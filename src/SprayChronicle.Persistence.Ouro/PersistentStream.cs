using System;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class PersistentStream : IStream
    {
        readonly IEventStoreConnection _eventStore;

        readonly UserCredentials _credentials;

        readonly ILocateTypes _typeLocator;

        readonly string _streamName;

        readonly string _groupName;

        public PersistentStream(
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            ILocateTypes typeLocator,
            string streamName,
            string groupName)
        {
            _eventStore = eventStore;
            _credentials = credentials;
            _typeLocator = typeLocator;
            _streamName = streamName;
            _groupName = groupName;
        }

        public void OnEvent(Action<object,DateTime> callback)
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
                    try {
                        var type = _typeLocator.Locate(resolvedEvent.Event.EventType);
                        if (null == type) {
                            subscription.Acknowledge(resolvedEvent);
                            return;
                        }

                        callback(
                            JsonConvert.DeserializeObject(
                                Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                                type
                            ),
                            resolvedEvent.Event.Created
                        );
                    
                        subscription.Acknowledge(resolvedEvent);
                    } catch (Exception error) {
                        Console.WriteLine("Persistent subscription {0}_{1} failure: {2}", _streamName, _groupName, error);
                        subscription.Fail(resolvedEvent, PersistentSubscriptionNakEventAction.Park, error.ToString());
                        return;
                    }
                },
                (subscription, reason, error) => {
                    Console.WriteLine("Persistent subscription {0}_{1} error: {2}, {3}", _streamName, _groupName, reason.ToString(), error.ToString());
                }
            );
        }
    }
}
