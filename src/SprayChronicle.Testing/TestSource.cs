using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Testing
{
    public sealed class TestSource<TTarget> : IEventSource<TTarget>
        where TTarget : class
    {
        private readonly ILogger<TTarget> _logger;
        
        private readonly BufferBlock<object> _queue = new BufferBlock<object>();
        
        private long _sequence;

        public TestSource() : this(new VoidLogger<TTarget>())
        {
        }

        public TestSource(ILogger<TTarget> logger)
        {
            _logger = logger;
        }

        public Task Publish(params object[] messages)
        {
            foreach (var message in messages) {
                _queue.Post(message);
                _logger.LogDebug($"Published {message.GetType().Name} to queue");
            }
            
            return Task.CompletedTask;
        }

        public Task Start()
        {
            return Task.CompletedTask;
        }

        public DomainMessage Convert(IMessagingStrategy<TTarget> strategy, object message)
        {
            if (null == message) {
                throw new InvalidDomainMessageException("You must provide a message for conversion");
            }
            
            if (!strategy.Resolves(message, DateTime.Now)) {
                throw new UnsupportedMessageException($"Message {message.GetType().Name} not resolved");
            }
            
            return new DomainMessage(
                _sequence++,
                DateTime.Now,
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