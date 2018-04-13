using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryPipeline<T> : IQueryPipeline where T : IQueryExecutor
    {
        private readonly ISourceBlock<DomainMessage> _domainMessages;
        
        private readonly ISourceBlock<QueryMessage> _queryMessages;

        private readonly T _processor;

        public QueryPipeline(
            ISourceBlock<DomainMessage> domainMessages,
            ISourceBlock<QueryMessage> queryMessages,
            T processor)
        {
            _domainMessages = domainMessages;
            _queryMessages = queryMessages;
            _processor = processor;
        }

        public async Task Process()
        {
            var batch = new BatchBlock<DomainMessage>(1000);
            var processor = new ActionBlock<DomainMessage[]>(ProcessMessages);
            
            _domainMessages.LinkTo(batch);
            batch.LinkTo(processor);
           
            await processor.Completion;
        }

        protected abstract Task ProcessMessages(DomainMessage[] domainMessages);
    }
}
