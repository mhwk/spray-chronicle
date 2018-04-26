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
        
        private readonly IMessagingStrategy<TProcessor> _strategy = new OverloadMessagingStrategy<TProcessor>(new ContextTypeLocator<TProcessor>());
        
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
            
            var converted = new TransformBlock<object,DomainMessage>(
                message => _source.Convert(_strategy, message),
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = 4
                }
            );
            var routed = new TransformBlock<DomainMessage,Processed>(
                async message => await Route(message),
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = 4
                }
            );
            var batched = new BatchBlock<Processed>(100, new GroupingDataflowBlockOptions
            {
//                Greedy = true,
//                BoundedCapacity = 20
            });
            var action = new ActionBlock<Processed[]>(async processed =>
            {
                await Apply(processed);
            });

            var timer = new Timer(time => {
                _logger.LogDebug($"TRIGGERING BATCH {batched.OutputCount}");
                batched.TriggerBatch();
            });
            
            var timeout = new TransformBlock<Processed,Processed>(message => {
                timer.Change(TimeSpan.FromSeconds(.01f), Timeout.InfiniteTimeSpan);
                return message;
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 4
            });
            
            _source.LinkTo(converted, new DataflowLinkOptions {
//                PropagateCompletion = true
            });
            converted.LinkTo(routed, new DataflowLinkOptions {
//                PropagateCompletion = true
            });
            routed.LinkTo(timeout, new DataflowLinkOptions {
//                PropagateCompletion = true
            });
            timeout.LinkTo(batched, new DataflowLinkOptions {
//                PropagateCompletion = true
            });
            batched.LinkTo(action, new DataflowLinkOptions {
//                PropagateCompletion = true
            });
            
            _logger.LogDebug($"Pipeline running...");
            await Task.WhenAll(_source.Start(), action.Completion);
            _logger.LogDebug($"Pipeline shutting down, waiting 1 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(1));
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

        private async Task<Processed> Route(DomainMessage domainMessage)
        {
            try {
                return await _strategy.Ask<Processed>(_processor, domainMessage.Payload, domainMessage.Epoch);
            } catch (Exception error) {
                _logger.LogDebug(error);
                return null;
            }
        }
        
        private async Task Apply(Processed[] processed)
        {
            _logger.LogDebug($"Applying {processed.Length} processed items");
            using (var session = _store.OpenAsyncSession()) {
                var identities = processed
                    .Where(p => null != p)
                    .Select(p => p.Identity)
                    .Where(i => i != null)
                    .Distinct()
                    .ToArray();
            
                var documents = await session.LoadAsync<TState>(identities);
                
//                _logger.LogDebug($"Working with identities ({string.Join(", ", identities)})");
//                _logger.LogDebug($"Found {documents.Count} documents...");
//                _logger.LogDebug($"Processing {processed.Length} items...");
            
                for (var i = 0; i < processed.Length; i++) {
                    _checkpoint++;
                    
                    try {
                        TState document = null;

                        if (null == processed[i]) {
                            _logger.LogDebug($"Skipping null at {i}");
                            continue;
                        }

                        if (null != processed[i].Identity) {
                            if (!documents.ContainsKey(processed[i].Identity)) {
                                throw new Exception($"Trying to update {processed[i].Identity} which doesn't exist");
                            }
                            
                            document = documents[processed[i].Identity];
                        }
                    
//                        _logger.LogDebug($" -> Processing {i}...");
                    
                        document = (TState) await processed[i].Do(document);
                    
                        await session.StoreAsync(document);
                    
                        var documentId = session.Advanced.GetDocumentId(document);

                        documents[documentId] = document;
                    }
                    catch (Exception error) {
                        _logger.LogCritical(error);
                        // @todo handle failure correctly
                    }
                }

                await SaveCheckpoint(session, _checkpoint);
            
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
                return 0;
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
