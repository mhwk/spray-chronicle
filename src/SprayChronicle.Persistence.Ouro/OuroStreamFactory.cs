using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroStreamFactory : IBuildStreams
    {
        private readonly ILogger<IEventStore> _logger;

        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        private readonly string _tenant;

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

        public IStream CatchUp(string streamName)
        {
            return new CatchUpStream(
                _logger,
                _eventStore,
                _credentials,
                streamName,
                _tenant
            );
        }

        public IStream Persistent(string streamName, string groupName)
        {
            return new PersistentStream(
                _logger,
                _eventStore,
                _credentials,
                streamName,
                groupName,
                _tenant
            );
        }
    }
}
