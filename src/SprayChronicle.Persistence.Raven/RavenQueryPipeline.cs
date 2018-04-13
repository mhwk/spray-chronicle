using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Raven.Client.Documents;
using SprayChronicle.EventSourcing;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenQueryPipeline<T> : QueryPipeline<T> where T : IQueryProcessor
    {
        private readonly IDocumentStore _store;

        public RavenQueryPipeline(
            ISourceBlock<DomainMessage> domainMessages,
            ISourceBlock<QueryMessage> queryMessages,
            IDocumentStore store,
            T processor) : base(domainMessages, queryMessages, processor)
        {
            _store = store;
        }

        protected override Task ProcessMessages(DomainMessage[] domainMessages)
        {
            using (var session = _store.OpenSession()) {
                
            }
        }      
    }
}
