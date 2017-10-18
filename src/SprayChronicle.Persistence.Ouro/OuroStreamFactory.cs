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

        readonly string _tenant;

        public OuroStreamFactory(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            string tenant)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _tenant = tenant;
        }

        public IStream CatchUp(string streamName, ILocateTypes typeLocator)
        {
            return new CatchUpStream(
                _logger,
                _eventStore,
                _credentials,
                typeLocator,
                streamName,
                _tenant
            );
        }

        public IStream Persistent(string streamName, string groupName, ILocateTypes typeLocator)
        {
            return new PersistentStream(
                _logger,
                _eventStore,
                _credentials,
                typeLocator,
                streamName,
                groupName,
                _tenant
            );
        }
    }
}
