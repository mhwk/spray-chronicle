using System;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class CatchUpStream : IStream
    {
        readonly IEventStoreConnection _eventStore;

        readonly ILocateTypes _typeLocator;

        readonly string _streamName;

        public CatchUpStream(
            IEventStoreConnection eventStore,
            ILocateTypes typeLocator,
            string streamName)
        {
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
                    throw new OuroException(string.Format("Catch up on {0} failure", _streamName), error);
                }
            );
        }
    }
}
