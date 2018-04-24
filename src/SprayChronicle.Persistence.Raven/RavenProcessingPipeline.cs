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
            TProcessor processor)
            : this(
                logger,
                store,
                sourceFactory,
                sourceOptions,
                processor,
                typeof(TProcessor).FullName)
        {
        }

        public RavenProcessingPipeline(
            ILogger<TProcessor> logger,
            IDocumentStore store,
            IEventSourceFactory sourceFactory,
            CatchUpOptions sourceOptions,
            TProcessor processor,
            string checkpointName)
        {
            _logger = logger;
            _store = store;
            _sourceFactory = sourceFactory;
            _sourceOptions = sourceOptions;
            _processor = processor;
            _checkpointName = checkpointName;
        }
        
        public async Task Start()
        {
            if (null != _source) {
                throw new Exception("Raven processing already started");
            }
            
            _checkpoint = await LoadCheckpoint();

            _source = _sourceFactory.Build<TProcessor,CatchUpOptions>(_sourceOptions.WithCheckpoint(_checkpoint));
            var converted = new TransformBlock<object,DomainMessage>(message => _source.Convert(_strategy, message));
            var routed = new TransformBlock<DomainMessage,RavenProcessed>(message => Route(message));
            var batched = new BatchBlock<RavenProcessed>(1000);
            var action = new ActionBlock<RavenProcessed[]>(processed => Apply(processed));

            var timer = new Timer(time => {
                batched.TriggerBatch();
            });
            
            var triggerBatch = new TransformBlock<RavenProcessed,RavenProcessed>(message =>
            {
                timer.Change(TimeSpan.FromSeconds(.1f), Timeout.InfiniteTimeSpan);
                return message;
            });
            
            _source.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(routed, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            routed.LinkTo(triggerBatch, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            triggerBatch.LinkTo(batched, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            batched.LinkTo(action, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            _logger.LogDebug($"Pipeline running...");
            await Task.WhenAll(_source.Start(), batched.Completion);
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

        private async Task<RavenProcessed> Route(DomainMessage domainMessage)
        {
            return await _strategy
                .Ask<RavenProcessed>(_processor, domainMessage.Payload, domainMessage.Epoch);
        }
        
        private async Task Apply(RavenProcessed[] processed)
        {
            using (var session = _store.OpenAsyncSession()) {
                try {
                    var identities = processed
                        .Select(p => p.Identity)
                        .Where(i => i != null)
                        .Distinct()
                        .ToArray();
                
                    var documents = await session.LoadAsync<TState>(identities);
                    
                    _logger.LogDebug($"Working with identities ({string.Join(", ", identities)})");
                    _logger.LogDebug($"Found {documents.Count} documents...");
                    _logger.LogDebug($"Processing {processed.Length} items...");
                
                    for (var i = 0; i < processed.Length; i++) {
                        TState document = null;

                        if (null != processed[i].Identity) {
                            if (!documents.ContainsKey(processed[i].Identity)) {
                                throw new Exception($"State with id {processed[i].Identity} not found in dictionary");
                            }
                            
                            document = documents[processed[i].Identity];
                        }
                    
                        _logger.LogDebug($"Processing {i} from {processed[i].Identity}");
                    
                        document = (TState) await processed[i].Do(document);
                    
                        await session.StoreAsync(document);
                    
                        var documentId = session.Advanced.GetDocumentId(document);

                        documents[documentId] = document;
                    
                        _checkpoint++;
                    }

                    await SaveCheckpoint(session, _checkpoint);
                
                    await session.SaveChangesAsync();
                    
                    _logger.LogInformation($"Processed {processed.Length} items");
                }
                catch (Exception error) {
                    _logger.LogCritical(error); // @todo handle failure correctly
                }
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
            var checkpoint = await session.LoadAsync<Checkpoint>(id);
            if (null != checkpoint) {
                _logger.LogDebug($"Saving checkpoint {checkpoint.Sequence}");
                checkpoint.Increase(sequence);
            } else {
                checkpoint = new Checkpoint(id);
                await session.StoreAsync(checkpoint);
            }
        }
    }
}
