using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public abstract class OuroSource<TSourceTarget> : IEventSource<DomainMessage>
    {
        protected readonly TransformBlock<ResolvedEvent,DomainMessage> _domainMessages;

        private readonly int _sleepMS = 100;

        private readonly int _minBufferLength = 1000;
        
        private readonly int _maxBufferLength = 40000;
        
        private bool _running;
        
        protected OuroSource()
        {
            _domainMessages = new TransformBlock<ResolvedEvent,DomainMessage>(
                resolvedEvent => ConsumeResolvedEvent(resolvedEvent)
            );
        }

        public async Task Start()
        {
            _running = true;
            while (_running) {
                await StartBuffering();
                
                while (_domainMessages.InputCount < _maxBufferLength) {
                    await Task.Delay(TimeSpan.FromMilliseconds(_sleepMS));
                }

                await StopBuffering();

                while (_domainMessages.InputCount > _minBufferLength) {
                    await Task.Delay(TimeSpan.FromMilliseconds(_sleepMS));
                }
            }
        }

        protected abstract Task StartBuffering();

        protected abstract Task StopBuffering();

        public Task Stop()
        {
            _running = false;
            return Task.CompletedTask;
        }

        protected static Task<DomainMessage> ConsumeResolvedEvent(ResolvedEvent resolvedEvent)
        {
            var type = MessageHandlingMetadata.For<TSourceTarget>(resolvedEvent.Event.EventType);
            if (null == type) {
                return null;
            }
            
            Console.WriteLine($"Consumed {resolvedEvent.Event.EventType} into {type}");
            return Task.FromResult(new DomainMessage(
                resolvedEvent.Event.EventNumber,
                resolvedEvent.Event.Created,
                JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                    type
                )
            ));
        }

        public void Complete()
        {
            _domainMessages.Complete();
        }

        public void Fault(Exception exception)
        {
            ((IDataflowBlock) _domainMessages).Fault(exception);
        }

        public Task Completion {
            get { return _domainMessages.Completion; }
        }

        public IDisposable LinkTo(ITargetBlock<DomainMessage> target, DataflowLinkOptions linkOptions)
        {
            return _domainMessages.LinkTo(target, linkOptions);
        }

        public DomainMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<DomainMessage> target, out bool messageConsumed)
        {
            return ((ISourceBlock<DomainMessage>)_domainMessages).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<DomainMessage> target)
        {
            return ((ISourceBlock<DomainMessage>)_domainMessages).ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<DomainMessage> target)
        {
            ((ISourceBlock<DomainMessage>)_domainMessages).ReleaseReservation(messageHeader, target);
        }
    }
}
