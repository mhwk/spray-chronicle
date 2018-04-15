using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SprayChronicle.QueryHandling
{
    public sealed class QueryQueue : IQueryQueue
    {
        private BufferBlock<QueryRequest> _buffer;

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, QueryRequest messageValue, ISourceBlock<QueryRequest> source,
            bool consumeToAccept)
        {
            return ((ITargetBlock<QueryRequest>)_buffer).OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void Complete()
        {
            _buffer.Complete();
        }

        public void Fault(Exception exception)
        {
            ((IDataflowBlock)_buffer).Fault(exception);
        }

        public Task Completion {
            get { return _buffer.Completion; }
        }

        public IDisposable LinkTo(ITargetBlock<QueryRequest> target, DataflowLinkOptions linkOptions)
        {
            return _buffer.LinkTo(target, linkOptions);
        }

        public QueryRequest ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<QueryRequest> target, out bool messageConsumed)
        {
            return ((ISourceBlock<QueryRequest>)_buffer).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<QueryRequest> target)
        {
            return ((ISourceBlock<QueryRequest>)_buffer).ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<QueryRequest> target)
        {
            ((ISourceBlock<QueryRequest>)_buffer).ReleaseReservation(messageHeader, target);
        }

        public Task Start()
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}
