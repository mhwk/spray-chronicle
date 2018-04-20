using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Testing
{
    public sealed class TestSource<TTarget> : IEventSource<TTarget>
        where TTarget : class
    {
        private readonly BufferBlock<object> _queue = new BufferBlock<object>();
        
        private long _sequence;
        
        public Task Publish(params object[] messages)
        {
            foreach (var message in messages) {
                _queue.Post(message);
            }
            
            return Task.CompletedTask;
        }

        public Task Start()
        {
            _queue.Complete();
            return Task.CompletedTask;
        }

        public DomainMessage Convert(IMessagingStrategy<TTarget> strategy, object message)
        {
            if (!strategy.Resolves(message)) {
                return null;
            }
            
            return new DomainMessage(
                _sequence++,
                new DateTime(),
                message
            );
        }

        public void Complete()
        {
            _queue.Complete();
        }

        public void Fault(Exception exception)
        {
            ((IDataflowBlock) _queue).Fault(exception);
        }

        public Task Completion
        {
            get { return _queue.Completion; }
        }

        public IDisposable LinkTo(ITargetBlock<object> target, DataflowLinkOptions linkOptions)
        {
            return _queue.LinkTo(target, linkOptions);
        }

        public object ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<object> target, out bool messageConsumed)
        {
            return ((ISourceBlock<object>)_queue).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<object> target)
        {
            return ((ISourceBlock<object>)_queue).ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<object> target)
        {
            ((ISourceBlock<object>)_queue).ReleaseReservation(messageHeader, target);
        }
    }
}