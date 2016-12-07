using Autofac;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.Example.Projection;

namespace SprayChronicle.Example
{
    public class ExampleProjectionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterEventHandler<NumberOfProductsInBasket,NumberOfProductsInBasketProjector>(builder);
        }

        void RegisterEventHandler<TProjection,TProjector>(ContainerBuilder builder) where TProjector : Projector<TProjection>
        {
            builder
                .Register<StreamEventHandler<TProjector>>(
                    c => c.Resolve<IBuildProjectorHandlers>().Build<TProjection,TProjector>(
                        c.Resolve<IBuildStreams>().CatchUp("$ce-SprayChronicle", new NamespaceTypeLocator("SprayChronicle.Example.Contracts.Events"))
                    )
                )
                .AsSelf()
                .As<IHandleStream>()
                .InstancePerDependency();
        }
    }
}
