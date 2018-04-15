using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq.Indexing;
using Raven.Client.Documents.Session;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenQueryPipeline<TProcessor,TState> : QueryPipeline<IAsyncDocumentSession,TProcessor>
        where TProcessor : RavenQueryProcessor<TProcessor,TState>
        where TState : class
    {
        private readonly IDocumentStore _store;

        public RavenQueryPipeline(
            IEventSource<DomainMessage> events,
            IQueryQueue queries,
            TProcessor processor,
            IDocumentStore store) : base(events, queries, processor)
        {
            _store = store;
        }

        protected override async Task ApplyEvents(EventProcessed[] processed)
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

        protected override async Task<object> ApplyQuery(QueryExecuted<IAsyncDocumentSession> execute)
        {
            using (var session = _store.OpenAsyncSession()) {
                return await execute.Do(session);
            }
        }
    }
}
