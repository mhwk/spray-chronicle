using System.Threading.Tasks.Dataflow;
using App.Metrics.Health;
using Autofac;
using Raven.Client.Documents;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => c.Resolve<ILoggerFactory>().Create<IDocumentStore>())
                .SingleInstance();

            builder
                .Register(c => {
                    var store = new DocumentStore {
                        Urls = new[] {
                            "http://ravendb"
                        },
                        Database = DatabaseName()
                    };
                    store.Initialize();
                    return store;
                })
                .AsSelf()
                .As<IDocumentStore>()
                .SingleInstance();
            
            builder
                .Register(c => new RavenHealthCheck(c.Resolve<IDocumentStore>()))
                .AsSelf()
                .As<HealthCheck>()
                .SingleInstance();
        }

        private static string DatabaseName()
        {
            return ChronicleServer.Env("RAVENDB_DB");
        }
        
        public sealed class QueryPipeline<TProcessor,TState> : Module
            where TProcessor : RavenQueryProcessor<TProcessor,TState>
            where TState : class
        {
            private readonly string _reference;

            private readonly string _streamName;

            public QueryPipeline(string stream)
                : this(typeof(TState).Name, stream)
            {}

            public QueryPipeline(string reference, string streamName)
            {
                _reference = reference;
                _streamName = streamName;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .Register(c => new RavenExecutionPipeline<TProcessor,TState>(
                        c.Resolve<IDocumentStore>(),
                        c.Resolve<TProcessor>()))
                    .AsSelf()
                    .As<IQueryPipeline>()
                    .As<IMessagingStrategyRouterSubscriber<IExecute>>()
                    .SingleInstance();
                
                builder
                    .Register(c => new RavenProcessingPipeline<TProcessor,TState>(
                        c.Resolve<IDocumentStore>(),
                        c.Resolve<IEventSourceFactory<DomainMessage>>().Build(new CatchUpOptions(
                            _streamName
                        )),
                        c.Resolve<TProcessor>()))
                    .AsSelf()
                    .As<IQueryPipeline>()
                    .As<IMessagingStrategyRouterSubscriber<IExecute>>()
                    .SingleInstance();
                
                builder
                    .RegisterType<TProcessor>()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                    .SingleInstance();
            }
        }
    }
}