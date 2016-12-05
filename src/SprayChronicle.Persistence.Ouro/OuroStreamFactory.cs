using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroStreamFactory : IBuildStreams
    {
        IEventStoreConnection _eventStore;

        UserCredentials _credentials;

        public OuroStreamFactory(IEventStoreConnection eventStore, UserCredentials credentials)
        {
            _eventStore = eventStore;
            _credentials = credentials;
        }

        public IStream CatchUp(string streamName, string @namespace)
        {
            return new CatchUpStream(_eventStore, new NamespaceTypeLocator(@namespace), streamName);
        }

        public IStream Persistent(string streamName, string groupName, string @namespace)
        {
            return new PersistentStream(_eventStore, _credentials, new NamespaceTypeLocator(@namespace), streamName, groupName);
        }
    }
}
