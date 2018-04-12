using System;
using App.Metrics.Health;
using Autofac;
using Raven.Client.Documents;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenDbModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(
                    c => new RavenDbRepositoryFactory(
                        c.Resolve<IDocumentStore>(),
                        c.Resolve<Microsoft.Extensions.Logging.ILoggerFactory>()
                    )
                )
                .AsSelf()
                .As<IBuildStatefulRepositories>()
                .SingleInstance();
            
            builder
                .Register(
                    c => new RavenDbProjectorFactory(
                        c.Resolve<RavenDbRepositoryFactory>()
                    )
                )
                .AsSelf()
                .As<IBuildProjectors>()
                .SingleInstance();
            
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
            return Environment.GetEnvironmentVariable("RAVENDB_DB");
        }
    }
}