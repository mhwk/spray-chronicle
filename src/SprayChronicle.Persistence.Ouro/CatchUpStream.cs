using System;
using System.Text;
using Microsoft.Extensions.Logging;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class CatchUpStream : IStream
    {
        readonly ILogger<IEventStore> _logger;

        readonly IEventStoreConnection _eventStore;

        readonly UserCredentials _credentials;

        readonly ILocateTypes _typeLocator;

        readonly string _streamName;

        public CatchUpStream(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            ILocateTypes typeLocator,
            string streamName)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _typeLocator = typeLocator;
            _streamName = streamName;
        }

        public void OnEvent(Action<object,DateTime> callback)
        {
            _eventStore.SubscribeToStreamFrom(
                _streamName,
                null,
                new CatchUpSubscriptionSettings(200, 100, false, true),
                (subscription, resolvedEvent) => OnEventAppeared(subscription, resolvedEvent, callback),
                OnLiveProcessingStarted,
                OnSubscriptionDropped,
                _credentials
            );
        }

        public void OnEventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent, Action<object,DateTime> callback)
        {
            var type = _typeLocator.Locate(resolvedEvent.Event.EventType);

            if (null == type) {
                _logger.LogDebug("[{0}] unknown type", _streamName);
                return;
            }

            callback(
                JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                    type
                ),
                resolvedEvent.Event.Created
            );
        }

        public void OnLiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {
            _logger.LogDebug("[{0}] in sync", _streamName);
        }

        public void OnSubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason reason, Exception error)
        {
            _logger.LogCritical("Catch up on {0} failure: {1}", _streamName, error.ToString());
        }
    }
}
