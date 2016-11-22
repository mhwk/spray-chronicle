using System;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class PersistentStream : IStream
    {
        readonly IEventStoreConnection _eventStore;

        readonly UserCredentials _credentials;

        readonly string _streamName;

        readonly string _groupName;

        public PersistentStream(IEventStoreConnection eventStore, UserCredentials credentials, string streamName, string groupName)
        {
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = streamName;
            _groupName = groupName;
        }

        public void Read(Action<string,string,DateTime> callback)
        {
             try {
                _eventStore.CreatePersistentSubscriptionAsync(
                    _streamName,
                    _groupName,
                    PersistentSubscriptionSettings.Create()
                        .ResolveLinkTos()
                        .StartFromBeginning()
                        .Build(),
                    _credentials
                ).Wait();
            } catch (AggregateException) {
                #if DEBUG
                Console.WriteLine("Persistent subscription {0}_{1} already exists!", _streamName, _groupName);
                #endif
            }

            _eventStore.ConnectToPersistentSubscription(
                _streamName,
                _groupName,
                (subscription, resolvedEvent) => {
                    callback(
                        resolvedEvent.Event.EventType,
                        Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                        resolvedEvent.Event.Created
                    );
                },
                (subscription, reason, error) => {
                    throw new OuroException(string.Format("Subscription on {0} failure", _streamName), error);
                }
            );
        }
    }
}
