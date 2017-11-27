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
    public sealed class PersistentStream : IStream
    {
        private readonly ILogger<IEventStore> _logger;

        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        private readonly string _streamName;

        private readonly string _groupName;

        private readonly string _tenant;

        public PersistentStream(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            string streamName,
            string groupName,
            string tenant)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = streamName;
            _groupName = groupName;
            _tenant = tenant;
        }

        public void Subscribe(Action<IMessage,DateTime> callback)
        {
             try {
                _eventStore.CreatePersistentSubscriptionAsync(
                    _streamName,
                    null == _tenant ? _groupName : string.Format("{0}_{1}", _tenant, _groupName),
                    PersistentSubscriptionSettings.Create()
                        .ResolveLinkTos()
                        .StartFromBeginning()
                        .Build(),
                    _credentials
                ).Wait();
            } catch (AggregateException) {
                _logger.LogDebug("Persistent subscription {0}_{1} already exists!", _streamName, _groupName);
            }

            _eventStore.ConnectToPersistentSubscription(
                _streamName,
                null == _tenant ? _groupName : string.Format("{0}_{1}", _tenant, _groupName),
                (subscription, resolvedEvent) => {
                    try {
                        var metadata = JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata));

                        if (metadata.Tenant == _tenant) {
                            try {
                                callback(
                                    new OuroMessage(resolvedEvent), 
                                    resolvedEvent.Event.Created
                                );
                            }
                            catch (UnhandledMessageException error) {
                                _logger.LogDebug("[{0}] message {1} not handled: {2}", _streamName, resolvedEvent.Event.EventType, error.ToString());
                            }
                        } else {
                            _logger.LogDebug("Skipping {0}, tenant {1} did not match {2}", resolvedEvent.Event.EventType, metadata.Tenant, _tenant);
                        }
                    
                        subscription.Acknowledge(resolvedEvent);
                    } catch (Exception error) {
                        _logger.LogWarning("Persistent subscription {0}_{1} failure: {2}", _streamName, _groupName, error.ToString());
                        subscription.Fail(resolvedEvent, PersistentSubscriptionNakEventAction.Park, error.ToString());
                        return;
                    }
                },
                (subscription, reason, error) => {
                    _logger.LogCritical("Persistent subscription {0}_{1} error: {2}, {3}", _streamName, _groupName, reason.ToString(), error.ToString());
                },
                _credentials
            );
        }
    }
}
