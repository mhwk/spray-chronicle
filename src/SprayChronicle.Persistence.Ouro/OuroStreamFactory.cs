using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroStreamFactory : IBuildStreams
    {
        readonly ILogger<IEventStore> _logger;

        readonly IEventStoreConnection _eventStore;

        readonly UserCredentials _credentials;

        public OuroStreamFactory(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
        }

        public IStream CatchUp(string streamName, string @namespace)
        {
            return new CatchUpStream(
                _logger,
                _eventStore,
                new NamespaceTypeLocator(@namespace),
                streamName
            );
        }

        public IStream Persistent(string streamName, string groupName, string @namespace)
        {
            return new PersistentStream(
                _logger,
                _eventStore,
                _credentials,
                new NamespaceTypeLocator(@namespace),
                streamName,
                groupName
            );
        }
    }
}
