using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenProcessingPipeline<TProcessor,TState> : IPipeline
        where TProcessor : class
        where TState : class
    {
        public string Description => $"Raven processing: {typeof(TProcessor).Name}";

        private const int BatchSize = 2000;
        private const int BatchTimeout = 200;
        private const int Parallelism = 4;
        
        private readonly IMailStrategy<TProcessor> _strategy = new OverloadMailStrategy<TProcessor>(new ContextTypeLocator<TProcessor>());
        
        private readonly ILogger<TProcessor> _logger;
        
        private readonly IEventSourceFactory _sourceFactory;
        
        private readonly CatchUpOptions _sourceOptions;

        private readonly TProcessor _processor;

        private readonly IDocumentStore _store;

        private IEventSource<TProcessor> _source;
        
        private readonly string _checkpointName;

        private long _checkpoint;

        public RavenProcessingPipeline(
            ILogger<TProcessor> logger,
            IDocumentStore store,
            IEventSourceFactory sourceFactory,
            CatchUpOptions sourceOptions,
            TProcessor processor,
            string checkpointName = null)
        {
            _logger = logger;
            _store = store;
            _sourceFactory = sourceFactory;
            _sourceOptions = sourceOptions;
            _processor = processor;
            _checkpointName = checkpointName ?? typeof(TState).Name;
        }
        
        public async Task Start()
        {
            if (null != _source) {
                throw new Exception("Raven processing already started");
            }
            
            _checkpoint = await LoadCheckpoint();

            _source = _sourceFactory
                .Build<TProcessor,CatchUpOptions>(_sourceOptions.WithCheckpoint(_checkpoint));
            
            var converted = new TransformBlock<object,DomainEnvelope>(
                message => {
                    try {
                        return _source.Convert(_strategy, message);
                    } catch (Exception error) {
//                        _logger.LogDebug(error);
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize
                }
            );
            var routed = new TransformBlock<DomainEnvelope,Processed>(
                async message => {
                    if (null == message) return null;
//                    _logger.LogDebug($"Route-{message.MessageName}-{message.Sequence}");
                    try {
                        return await _strategy.Ask<Processed>(_processor, message.Message, message.Epoch);
                    } catch (Exception error) {
                        _logger.LogDebug(error);
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize
                }
            );
            var batched = new BatchBlock<Processed>(BatchSize, new GroupingDataflowBlockOptions {
//                Greedy = true,
                BoundedCapacity = BatchSize
            });
            var action = new ActionBlock<Processed[]>(
                async processed => {
                    try {
                        await Apply(processed);
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        throw;
                    }
                }
            );

            var timer = new Timer(time => {
//                _logger.LogDebug($"TRIGGERING BATCH {batched.OutputCount}");
                batched.TriggerBatch();
            });
            
            var timeout = new TransformBlock<Processed,Processed>(
                processed => {
                    timer.Change(TimeSpan.FromMilliseconds(BatchTimeout), Timeout.InfiniteTimeSpan);
//                    _logger.LogDebug($"Pre-{processed.GetType().Name}");
                    return processed;
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize
                }
            );
            
            _source.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(routed, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            routed.LinkTo(timeout, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            timeout.LinkTo(batched, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            batched.LinkTo(action, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            _logger.LogDebug($"Starting source...");
            await _source.Start();
            _logger.LogDebug($"Pipeline running...");
            await action.Completion;
            _logger.LogDebug($"Pipeline shut down");
        }

        public Task Stop()
        {
            if (null == _source) {
                throw new Exception("Raven processing not started");
            }
            
            _logger.LogDebug($"Pipeline stopping...");
            _source.Complete();
            return _source.Completion;
        }

        private async Task Apply(Processed[] processed)
        {
            _logger.LogDebug($"Applying {processed.Length} processed items");
            using (var session = _store.OpenAsyncSession()) {
                var identities = processed
                    .Where(p => p != null)
                    .Select(p => p.Identity)
                    .Distinct()
                    .ToArray();
            
                var documents = await session.LoadAsync<TState>(identities);
                
//                _logger.LogDebug($"Working with identities ({string.Join(", ", identities)})");
//                _logger.LogDebug($"Found {documents.Count} documents...");
//                _logger.LogDebug($"Processing {processed.Length} items...");
            
                for (var i = 0; i < processed.Length; i++) {
                    if (null == processed[i]) {
//                        _logger.LogDebug($"Skipping null at {i}");
                        continue;
                    }
                
//                        _logger.LogDebug($" -> Processing {i}...");

                    try {
                        documents[processed[i].Identity] = (TState) await processed[i].Do(documents[processed[i].Identity]);
                        if (null == documents[processed[i].Identity]) {
                            throw new Exception($"Null value has been set");
                        }
                    } catch (ArgumentException error) {
                        _logger.LogCritical(error, $"At {_checkpoint + i}");
                        throw;
                    }

                    var cancellation = new CancellationTokenSource();
                    
                    await session.StoreAsync(
                        documents[processed[i].Identity],
                        processed[i].Identity,
                        cancellation.Token
                    );
                }

                await SaveCheckpoint(session, _checkpoint += processed.Length);
            
                await session.SaveChangesAsync();
                
                _logger.LogInformation($"Processed {processed.Length} items");
            }
        }

        private async Task<long> LoadCheckpoint()
        {
            using (var session = _store.OpenAsyncSession()) {
                return await LoadCheckpoint(session);
            }
        }

        private async Task<long> LoadCheckpoint(IAsyncDocumentSession session)
        {
            var id = $"Checkpoint/{_checkpointName}";
            var checkpoint = await session.LoadAsync<Checkpoint>(id);

            if (null == checkpoint) {
                _logger.LogDebug("Starting from the beginning");
                return -1;
            }
            
            _logger.LogDebug($"Starting from checkpoint {checkpoint.Sequence}");
            return checkpoint.Sequence;
        }

        private async Task SaveCheckpoint(IAsyncDocumentSession session, long sequence)
        {
            var id = $"Checkpoint/{_checkpointName}";
            _logger.LogDebug($"Saving checkpoint {id} at {sequence}");
            
            var checkpoint = await session.LoadAsync<Checkpoint>(id);
            if (null != checkpoint) {
                checkpoint.Increase(sequence);
            } else {
                checkpoint = new Checkpoint(id);
                await session.StoreAsync(checkpoint);
            }
        }
    }
}
