using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class CatchUpSource<T> : IMessageSource<DomainMessage>
    {
        private readonly IEventStoreConnection _eventStore;
        
        private readonly string _streamName;
        
        private readonly int _sleepMS = 100;

        private readonly int _minBufferLength = 1000;
        
        private readonly int _maxBufferLength = 40000;
        
        private bool _running;

        private Exception _error;
        
        private bool _liveProcessing;

        private long _checkPoint;
        
        private TransformBlock<ResolvedEvent,DomainMessage> _domainMessages;

        public CatchUpSource( IEventStoreConnection eventStore, string streamName)
        {
            _eventStore = eventStore;
            _streamName = streamName;
            _domainMessages = new TransformBlock<ResolvedEvent,DomainMessage>(
                resolvedEvent => ConsumeResolvedEvent(resolvedEvent)
            );
        }

        public async Task Start()
        {
            _running = true;
            while (_running) {
                var subscription = Subscribe();
                
                while (_domainMessages.InputCount < _maxBufferLength) {
                    await Task.Delay(TimeSpan.FromMilliseconds(_sleepMS));
                }
            
                subscription.Stop();

                while (_domainMessages.InputCount > _minBufferLength) {
                    await Task.Delay(TimeSpan.FromMilliseconds(_sleepMS));
                }
            }
        }

        public Task Stop()
        {
            _running = false;
            return Task.CompletedTask;
        }

        private EventStoreCatchUpSubscription Subscribe()
        {
            Console.WriteLine($"Subscribing to stream {_streamName}");
            return _eventStore.SubscribeToStreamFrom(
                _streamName,
                _checkPoint,
                CatchUpSubscriptionSettings.Default,
                EventAppeared,
                LiveProcessingStarted,
                SubscriptionDropped
            );
        }

        private Task EventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            _checkPoint++;
            _domainMessages.Post(resolvedEvent);
            
            Console.WriteLine($"Produced {resolvedEvent.Event.EventType}");

            return Task.CompletedTask;
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {
            Console.WriteLine($"Live processing {_streamName}");
            
            _liveProcessing = true;
        }

        private void SubscriptionDropped(
            EventStoreCatchUpSubscription subscription,
            SubscriptionDropReason reason,
            Exception error)
        {
            Console.WriteLine($"Subscription to {_streamName} dropped");
            
            _liveProcessing = false;
            _error = error;
        }

        private static Task<DomainMessage> ConsumeResolvedEvent(ResolvedEvent resolvedEvent)
        {
            var type = MessageHandlingMetadata.For<T>(resolvedEvent.Event.EventType);
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
