using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Raven.Client.Documents;
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
        
        private readonly ISourceBlock<DomainMessage> _source;
        
        private readonly TProcessor _processor;
        
        private readonly IDocumentStore _store;

        public RavenProcessingPipeline(IDocumentStore store, ISourceBlock<DomainMessage> source, TProcessor processor)
        {
            _source = source;
            _processor = processor;
            _store = store;
        }
        
        public Task Start()
        {
            var routed = new TransformBlock<DomainMessage,RavenProcessed>(message => Route(message));
            var batched = new BatchBlock<RavenProcessed>(1000);
            var action = new ActionBlock<RavenProcessed[]>(processed => Apply(processed));

            _source.LinkTo(routed, new DataflowLinkOptions {
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

            return action.Completion;
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
                }

                await session.SaveChangesAsync();
            }
        }

    }
}
