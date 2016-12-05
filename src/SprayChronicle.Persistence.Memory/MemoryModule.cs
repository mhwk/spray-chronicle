using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.EventHandling.Projecting;
using SprayChronicle.EventSourcing;

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
                .As<IBuildProjectionRepositories>()
                .SingleInstance();
            
            builder
                .Register<MemoryProjectorFactory>(
                    c => new MemoryProjectorFactory(
                        c.Resolve<ILogger<IStream>>(),
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
        }
    }
}
