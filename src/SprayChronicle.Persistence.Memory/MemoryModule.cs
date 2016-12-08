using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.EventSourcing;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<MemoryRepositoryFactory>(
                    c => new MemoryRepositoryFactory()
                )
                .AsSelf()
                .As<IBuildStatefulRepositories>()
                .SingleInstance();
            
            builder
                .Register<MemoryProjectorFactory>(
                    c => new MemoryProjectorFactory(
                        c.Resolve<ILoggerFactory>(),
                        c.Resolve<MemoryRepositoryFactory>()
                    )
                )
                .AsSelf()
                .As<IBuildProjectors>()
                .SingleInstance();

            builder
                .Register<MemoryEventStore>(c => new MemoryEventStore())
                .AsSelf()
                .As<IEventStore>()
                .SingleInstance();
            
            builder
                .Register<MemoryStreamFactory>(c => new MemoryStreamFactory(
                    c.Resolve<MemoryEventStore>()
                ))
                .AsSelf()
                .As<IBuildStreams>()
                .SingleInstance();
            
            builder
                .Register<ILoggerFactory>(c => new LoggerFactory())
                .SingleInstance();
        }
    }
}
