using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroSourceFactory : IEventSourceFactory<DomainMessage>
    {
        private readonly ILogger<IEventStore> _logger;

        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        public OuroSourceFactory(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
        }

        public IEventSource<DomainMessage> Build<TOptions>(TOptions options)
        {
            var persistentOptions = options as PersistentOptions;
            if (null != persistentOptions) {
                return new PersistentSource<DomainMessage>(
                    _logger,
                    _eventStore,
                    _credentials,
                    persistentOptions.StreamName,
                    persistentOptions.GroupName
                );
            }
            
            var catchUpOptions = options as CatchUpOptions;
            if (null != catchUpOptions) {
                return new CatchUpSource<DomainMessage>(
                    _logger,
                    _eventStore,
                    _credentials,
                    catchUpOptions.StreamName
                );
            }

            throw new SourceBuildException($"SourceOptions {typeof(TOptions)} not supported");
        }
    }
}
