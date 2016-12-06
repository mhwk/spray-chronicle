using System;
using System.Text;
using Microsoft.Extensions.Logging;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class CatchUpStream : IStream
    {
        readonly ILogger<IEventStore> _logger;

        readonly IEventStoreConnection _eventStore;

        readonly ILocateTypes _typeLocator;

        readonly string _streamName;

        public CatchUpStream(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            ILocateTypes typeLocator,
            string streamName)
        {
            _logger = logger;
            _eventStore = eventStore;
            _typeLocator = typeLocator;
            _streamName = streamName;
        }

        public void OnEvent(Action<object,DateTime> callback)
        {
            _eventStore.SubscribeToStreamFrom(
                _streamName,
                null,
                new CatchUpSubscriptionSettings(200, 100, false, true),
                (subscription, resolvedEvent) => {
                    var type = _typeLocator.Locate(resolvedEvent.Event.EventType);

                    if (null == type) {
                        return;
                    }

                    callback(
                        JsonConvert.DeserializeObject(
                            Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                            type
                        ),
                        resolvedEvent.Event.Created
                    );
                },
                (subscription) => {
                    #if DEBUG
                    Console.WriteLine("Stream {0} in sync", _streamName);
                    #endif
                },
                (subscription, reason, error) => {
                    Console.WriteLine("Catch up on {0} failure: {1}", _streamName, error.ToString());
                }
            );
        }
    }
}
