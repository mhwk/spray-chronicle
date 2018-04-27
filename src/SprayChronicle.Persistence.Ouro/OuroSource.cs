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
        
        protected readonly BufferBlock<object> Queue = new BufferBlock<object>();

        private const int SleepMs = 100;

        private const int MinBufferLength = 1000;

        private const int MaxBufferLength = 40000;

        private bool _running;

        public OuroSource(ILogger<TTarget> logger)
        {
            _logger = logger;
        }

        public async Task Start()
        {
            while (!Queue.Completion.IsCompleted && !Queue.Completion.IsFaulted && !Queue.Completion.IsCanceled) {
                // @todo figure out other way than polling
                await StartBuffering();
                _logger.LogDebug("Buffer started");
            
                while (Queue.Count < MaxBufferLength && !Queue.Completion.IsCompleted && !Queue.Completion.IsFaulted && !Queue.Completion.IsCanceled) {
                    await Task.Delay(TimeSpan.FromMilliseconds(SleepMs));
                }

                await StopBuffering();
                _logger.LogDebug("Buffer stopped");

                while (Queue.Count > MinBufferLength) {
                    await Task.Delay(TimeSpan.FromMilliseconds(SleepMs));
                }
            }

            if (Queue.Completion.IsFaulted) {
                throw Queue.Completion.Exception;
            }
            
            _logger.LogDebug("DONE");
        }

        public DomainMessage Convert(IMessagingStrategy<TTarget> strategy, object message)
        {
            if (!(message is ResolvedEvent resolvedEvent)) {
                throw new ArgumentException($"Message of type {message.GetType()} is expected to be a {typeof(ResolvedEvent)}");
            }
            
            var type = strategy.ToType(resolvedEvent.Event.EventType);
            
            return new DomainMessage(
                resolvedEvent.Event.EventNumber,
                resolvedEvent.Event.Created,
                JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                    type
                )
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
