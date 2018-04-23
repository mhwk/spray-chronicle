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

        private long _checkpoint;

        public RavenProcessingPipeline(
            ILogger<TProcessor> logger,
            IDocumentStore store,
            IEventSourceFactory sourceFactory,
            CatchUpOptions sourceOptions,
            TProcessor processor)
        {
            _logger = logger;
            _store = store;
            _sourceFactory = sourceFactory;
            _sourceOptions = sourceOptions;
            _processor = processor;
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

            _logger.LogDebug($"Raven processing pipeline running...");
            await Task.WhenAll(_source.Start(), batched.Completion);
        }

        public Task Stop()
        {
            if (null == _source) {
                throw new Exception("Raven processing not started");
            }
            
            _logger.LogDebug($"Raven processing pipeline stopping...");
            _source.Complete();
            return _source.Completion;
        }

        private async Task<RavenProcessed> Route(DomainMessage domainMessage)
        {
            return await _strategy
                .Ask<RavenProcessed>(_processor, domainMessage.Payload, domainMessage.Epoch)
                .ConfigureAwait(false);
        }
        
        private async Task Apply(RavenProcessed[] processed)
        {
            using (var session = _store.OpenAsyncSession()) {
                var identities = processed.Select(p => p.Identity).Distinct().ToArray();
                
                _logger.LogDebug($"Working with identities {string.Join(", ", identities)}");
                
                var documents = await session.LoadAsync<TState>(identities);
                var ordered = processed
                    .Select(p => {
                        documents.TryGetValue(p.Identity, out var document);
                        return document;
                    })
                    .ToArray();

                for (var i = 0; i < processed.Length; i++) {
                    ordered[i] = processed[i].Do(ordered[i]) as TState;
                    await session.StoreAsync(ordered[i]);
                    _checkpoint++;
                }

                await SaveCheckpoint(session, _checkpoint);
                
                await session.SaveChangesAsync();
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
            var id = $"Checkpoint/{typeof(TState).Name}";
            var checkpoint = await session.LoadAsync<Checkpoint>(id);

            if (null == checkpoint) {
                _logger.LogDebug("No checkpoint");
                return 0;
            }
            
            _logger.LogDebug($"Loaded checkpoint {checkpoint.Sequence}");
            return checkpoint.Sequence;
        }

        private async Task SaveCheckpoint(IAsyncDocumentSession session, long sequence)
        {
            var id = $"Checkpoint/{typeof(TState).Name}";
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
