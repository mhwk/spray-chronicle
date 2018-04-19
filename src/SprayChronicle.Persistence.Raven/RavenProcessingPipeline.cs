using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenProcessingPipeline<TProcessor,TState> : IPipeline
        where TProcessor : class
        where TState : class
    {
        public string Description => $"Raven processing: {typeof(TProcessor).Name}";
        
        private readonly IMessagingStrategy<TProcessor> _strategy = new OverloadMessagingStrategy<TProcessor>(new ContextTypeLocator<TProcessor>());
        
        private readonly IEventSourceFactory _sourceFactory;
        
        private readonly CatchUpOptions _sourceOptions;

        private readonly TProcessor _processor;
        
        private readonly IDocumentStore _store;

        private long _checkpoint;

        public RavenProcessingPipeline(
            IDocumentStore store,
            IEventSourceFactory sourceFactory,
            CatchUpOptions sourceOptions,
            TProcessor processor)
        {
            _store = store;
            _sourceFactory = sourceFactory;
            _sourceOptions = sourceOptions;
            _processor = processor;
        }
        
        public async Task Start()
        {
            _checkpoint = await LoadCheckpoint();

            var source = _sourceFactory.Build<TProcessor,CatchUpOptions>(_sourceOptions.WithCheckpoint(_checkpoint));
            var converted = new TransformBlock<object,DomainMessage>(message => source.Convert(_strategy, message));
            var routed = new TransformBlock<DomainMessage,RavenProcessed>(message => Route(message));
            var batched = new BatchBlock<RavenProcessed>(1000);
            var action = new ActionBlock<RavenProcessed[]>(processed => Apply(processed));

            source.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(routed, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            routed.LinkTo(batched, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            routed.LinkTo(batched, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            batched.LinkTo(action, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await Task.WhenAll(source.Start(), action.Completion);
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
            return checkpoint?.Sequence ?? 0;
        }

        private async Task SaveCheckpoint(IAsyncDocumentSession session, long sequence)
        {
            var id = $"Checkpoint/{typeof(TState).Name}";
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
