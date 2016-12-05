using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Coordination;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example
{
    public class ExampleCoordinationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<BasketHandler>(c => new BasketHandler(
                    new EventSourcedRepository<Basket>(
                        c.Resolve<IEventStore>()
                    )
                ))
                .As<IHandleCommand>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
