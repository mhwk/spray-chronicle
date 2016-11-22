using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class StreamFactory
    {
        IEventStoreConnection _eventStore;

        UserCredentials _credentials;

        public StreamFactory(IEventStoreConnection eventStore, UserCredentials credentials)
        {
            _eventStore = eventStore;
            _credentials = credentials;
        }

        public CatchUpStream CatchUp(string streamName)
        {
            return new CatchUpStream(_eventStore, streamName);
        }

        public PersistentStream Persistent(string streamName, string groupName)
        {
            return new PersistentStream(_eventStore, _credentials, streamName, groupName);
        }
    }
}
