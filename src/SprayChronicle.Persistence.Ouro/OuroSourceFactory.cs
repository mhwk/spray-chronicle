using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroSourceFactory : IEventSourceFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        public OuroSourceFactory(
            ILoggerFactory loggerFactory,
            IEventStoreConnection eventStore,
            UserCredentials credentials)
        {
            _loggerFactory = loggerFactory;
            _eventStore = eventStore;
            _credentials = credentials;
        }

        public IEventSource<TTarget> Build<TTarget,TOptions>(TOptions options)
            where TTarget : class
        {
            var readForwardOptions = options as ReadForwardOptions;
            if (null != readForwardOptions) {
                return new ReadForwardSource<TTarget>(
                    _loggerFactory.Create<TTarget>(),
                    _eventStore,
                    _credentials,
                    readForwardOptions
                );
            }
            
            var catchUpOptions = options as CatchUpOptions;
            if (null != catchUpOptions) {
                return new CatchUpSource<TTarget>(
                    _loggerFactory.Create<TTarget>(),
                    _eventStore,
                    _credentials,
                    catchUpOptions
                );
            }
            
            var persistentOptions = options as PersistentOptions;
            if (null != persistentOptions) {
                return new PersistentSource<TTarget>(
                    _loggerFactory.Create<TTarget>(),
                    _eventStore,
                    _credentials,
                    persistentOptions
                );
            }
            

            throw new SourceBuildException($"SourceOptions {typeof(TOptions)} not supported");
        }
    }
}
