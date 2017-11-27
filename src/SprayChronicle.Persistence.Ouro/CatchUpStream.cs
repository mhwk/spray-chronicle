using System;
using System.Text;
using Microsoft.Extensions.Logging;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class CatchUpStream : IStream
    {
        private readonly ILogger<IEventStore> _logger;

        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        private readonly string _streamName;

        private readonly string _tenant;

        public CatchUpStream(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            string streamName,
            string tenant)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = streamName;
            _tenant = tenant;
        }

        public void Subscribe(Action<object,DateTime> callback)
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

        private void OnEventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent, Action<object,DateTime> callback)
        {
            var metadata = JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata));
            if (metadata.Tenant != _tenant) {
                _logger.LogDebug("Skipping {0}, tenant {1} did not match {2}", resolvedEvent.Event.EventType, metadata.Tenant, _tenant);
                return;
            }

            try {
                callback(
                    new OuroMessage(resolvedEvent),
                    resolvedEvent.Event.Created
                );
            } catch (UnhandledMessageException error) {
                _logger.LogDebug("[{0}] message {1} not handled: {2}", _streamName, resolvedEvent.Event.EventType, error.ToString());
            }
        }

        private void OnLiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {
            _logger.LogDebug("[{0}] in sync", _streamName);
        }

        private void OnSubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason reason, Exception error)
        {
            _logger.LogCritical("Catch up on {0} failure: {1}", _streamName, error.ToString());
        }
    }
}
