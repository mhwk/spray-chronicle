using System;
using System.Data.SqlTypes;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking;
using Newtonsoft.Json;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class ProcessingPipeline<T> : IEventProcessor where T : IProcessEvents
    {
        private readonly T _handler;
        
        private readonly IEventStoreConnection _eventStore;
        
        private readonly string _streamName;

        private readonly BufferBlock<ResolvedEvent> _buffer = new BufferBlock<ResolvedEvent>();

        private readonly int _sleepMS = 100;

        private readonly int _minBufferLength = 1000;
        
        private readonly int _maxBufferLength = 40000;

        private Exception _error;
        
        private bool _liveProcessing;

        private long _checkPoint;

        public ProcessingPipeline(
            T handler,
            IEventStoreConnection eventStore,
            string streamName)
        {
            _handler = handler;
            _eventStore = eventStore;
            _streamName = streamName;
        }

        public async Task Process()
        {
            var resolved = new BufferBlock<ResolvedEvent>();
            var messages = new BufferBlock<DomainMessage>();
            
            var producer = ProduceResolvedEvents(resolved);
            var converter = ConsumeResolvedEvents(resolved, messages);
            var processor = ConsumeDomainMessages(messages);

            await Task.WhenAll(converter, producer, processor);
        }

        private async Task ProduceResolvedEvents(BufferBlock<ResolvedEvent> buffer)
        {
            while (true) {
                var subscription = Subscribe();

                while (buffer.Count < _maxBufferLength) {
                    await Task.Delay(TimeSpan.FromMilliseconds(_sleepMS));
                }
            
                subscription.Stop();

                while (buffer.Count > _minBufferLength) {
                    await Task.Delay(TimeSpan.FromMilliseconds(_sleepMS));
                }
            }
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
            _buffer.Post(resolvedEvent);
            
            Console.WriteLine($"Produced {resolvedEvent.Event.EventType}");

            return Task.CompletedTask;
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {
            _liveProcessing = true;
            Console.WriteLine($"Live processing {_streamName}");
        }

        private void SubscriptionDropped(
            EventStoreCatchUpSubscription subscription,
            SubscriptionDropReason reason,
            Exception error)
        {
            _liveProcessing = false;
            _error = error;
            Console.WriteLine($"Subscription to {_streamName} dropped");
        }

        private async Task ConsumeResolvedEvents(
            IReceivableSourceBlock<ResolvedEvent> resolved,
            ITargetBlock<DomainMessage> messages)
        {
            while (true) {
                if (!resolved.TryReceive(out var resolvedEvent)) {
                    await Task.Delay(_sleepMS);
                    continue;
                }

                var type = MessageHandlingMetadata.For<T>(resolvedEvent.Event.EventType);
                if (null == type) {
                    continue;
                }

                messages.Post(new DomainMessage(
                    resolvedEvent.Event.EventNumber,
                    resolvedEvent.Event.Created,
                    JsonConvert.DeserializeObject(
                        Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                        type
                    )
                ));
                Console.WriteLine($"Converted {resolvedEvent.Event.EventType} into {type}");
            }
        }
        
        private async Task ConsumeDomainMessages(IReceivableSourceBlock<DomainMessage> messages)
        {
            while (true) {
                if (!messages.TryReceive(out var domainMessage)) {
                    await Task.Delay(_sleepMS);
                    continue;
                }
                
                _handler.Process(domainMessage.Payload(), domainMessage.Epoch);
                Console.WriteLine($"Processed {domainMessage.Payload().GetType()}");
            }
        }
    }
}
