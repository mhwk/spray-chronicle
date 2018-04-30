using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public abstract class OuroSource<TTarget> : IEventSource<TTarget>
        where TTarget : class
    {
        private readonly ILogger<TTarget> _logger;
        
        private readonly string _causationId;

        protected readonly BufferBlock<object> Queue = new BufferBlock<object>(new DataflowBlockOptions {
            BoundedCapacity = 2000
        });

        private bool _running;

        public OuroSource(ILogger<TTarget> logger, string causationId)
        {
            _logger = logger;
            _causationId = causationId;
        }

        public async Task Start()
        {
            await StartBuffering();

            await Queue.Completion;
            
            _logger.LogDebug($"{GetType().Name} done streaming messages");
        }

        public DomainEnvelope Convert(IMailStrategy<TTarget> strategy, object message)
        {
            if (!(message is ResolvedEvent resolvedEvent)) {
                throw new ArgumentException($"Message of type {message.GetType()} is expected to be a {typeof(ResolvedEvent)}");
            }
            
            var type = strategy.ToType(resolvedEvent.Event.EventType);
            var metadata = JsonConvert.DeserializeObject<Metadata>(
                Encoding.UTF8.GetString(resolvedEvent.Event.Metadata)
            );

            if (null != _causationId && _causationId == metadata.CausationId) {
                Console.WriteLine("Message {_causationId} has already been handled");
                throw new IdempotencyException($"Message {_causationId} has already been handled");
            }
            
            return new DomainEnvelope(
                metadata.MessageId,
                metadata.CausationId,
                metadata.CorrelationId,
                resolvedEvent.Event.EventNumber,
                JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                    type
                ),
                resolvedEvent.Event.Created
            );
        }

        protected abstract Task StartBuffering();

        protected abstract Task StopBuffering();

        public void Complete()
        {
            _logger.LogDebug("Completing which should not happen");
            Queue.Complete();
        }

        public void Fault(Exception exception)
        {
            ((IDataflowBlock) Queue).Fault(exception);
        }

        public Task Completion
        {
            get { return Queue.Completion; }
        }

        public IDisposable LinkTo(ITargetBlock<object> target, DataflowLinkOptions linkOptions)
        {
            return Queue.LinkTo(target, linkOptions);
        }

        public object ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<object> target, out bool messageConsumed)
        {
            return ((ISourceBlock<object>)Queue).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<object> target)
        {
            return ((ISourceBlock<object>)Queue).ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<object> target)
        {
            ((ISourceBlock<object>)Queue).ReleaseReservation(messageHeader, target);
        }
    }
}
