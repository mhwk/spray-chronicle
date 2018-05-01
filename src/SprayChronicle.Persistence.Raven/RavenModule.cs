using App.Metrics.Health;
using Autofac;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;
using ConcurrencyException = Raven.Client.Exceptions.ConcurrencyException;

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
                .Register(c =>
                {
                    var database = ChronicleServer.Env(
                        "RAVENDB_DB",
                        ChronicleServer.Env(
                            "HOSTNAME",
                            "default"
                        )
                    );
                    
                    var store = new DocumentStore {
                        Urls = new[] {
                            ChronicleServer.Env("RAVENDB_HOST", "http://localhost:8080")
                        },
                        Database = database
                    };
                    store.Initialize();

                    try {
                        store.Maintenance.Server.Send(new CreateDatabaseOperation(
                            new DatabaseRecord(database)
                        ));
                    } catch (ConcurrencyException) {
                        c.Resolve<ILoggerFactory>().Create<RavenModule>().LogDebug($"Database {database} already exist");
                    }
                    
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

        public sealed class QueryPipeline<TProcessor,TState> : Module
            where TProcessor : RavenQueries<TProcessor,TState>
            where TState : class
        {
            private readonly StreamOptions _streamOptions;
            private readonly string _checkpointName;

            public QueryPipeline(StreamOptions streamOptions = null, string checkpointName = null)
            {
                _streamOptions = streamOptions;
                _checkpointName = checkpointName;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .RegisterType<TProcessor>()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                    .OnActivated(e => IndexCreation.CreateIndexes(typeof(TProcessor).Assembly, e.Context.Resolve<IDocumentStore>()))
                    .SingleInstance();
                
                builder
                    .Register(c => new RavenExecutionPipeline<TProcessor,TState>(
                        c.Resolve<ILoggerFactory>().Create<TProcessor>(),
                        c.Resolve<IDocumentStore>(),
                        c.Resolve<TProcessor>()))
                    .AsSelf()
                    .As<IPipeline>()
                    .As<IMailStrategyRouterSubscriber<IExecute>>()
                    .SingleInstance();

                if (null != _streamOptions) {
                    builder
                        .Register(c => new RavenProcessingPipeline<TProcessor,TState>(
                            c.Resolve<ILoggerFactory>().Create<TProcessor>(),
                            c.Resolve<IDocumentStore>(),
                            c.Resolve<IEventSourceFactory>(),
                            new CatchUpOptions(_streamOptions),
                            c.Resolve<TProcessor>(),
                            _checkpointName
                        ))
                        .AsSelf()
                        .As<IPipeline>()
                        .SingleInstance();
                }
            }
        }
    }
}