using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new MemoryEventStore())
                .AsSelf()
                .As<IEventStore>()
                .SingleInstance();
        }
    }
}
