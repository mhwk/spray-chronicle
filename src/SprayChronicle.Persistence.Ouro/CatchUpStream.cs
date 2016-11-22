using System;
using System.Text;
using EventStore.ClientAPI;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class CatchUpStream : IStream
    {
        readonly IEventStoreConnection _eventStore;

        readonly string _streamName;

        public CatchUpStream(IEventStoreConnection eventStore, string streamName)
        {
            _eventStore = eventStore;
            _streamName = streamName;
        }

        public void Read(Action<string,string,DateTime> callback)
        {
            _eventStore.SubscribeToStreamFrom(
                _streamName,
                null,
                new CatchUpSubscriptionSettings(200, 100, false, true),
                (subscription, resolvedEvent) => {
                    callback(
                        resolvedEvent.Event.EventType,
                        Encoding.UTF8.GetString(resolvedEvent.Event.Data),
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
