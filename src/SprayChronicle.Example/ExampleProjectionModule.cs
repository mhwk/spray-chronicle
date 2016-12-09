using Autofac;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;
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
                .Register<IStatefulRepository<NumberOfProductsInBasket>>(
                    c => c.Resolve<IBuildStatefulRepositories>().Build<NumberOfProductsInBasket>()
                )
                .InstancePerDependency();
            
            builder
                .Register<StreamEventHandler<NumberOfProductsInBasketProjector>>(
                    c => c.Resolve<IBuildProjectorHandlers>().Build<NumberOfProductsInBasket,NumberOfProductsInBasketProjector>(
                        c.Resolve<IBuildStreams>().CatchUp("$ce-SprayChronicle", new NamespaceTypeLocator("SprayChronicle.Example.Contracts.Events")),
                        c.Resolve<IStatefulRepository<NumberOfProductsInBasket>>()
                    )
                )
                .AsSelf()
                .As<IHandleStream>()
                .InstancePerDependency();
            
            builder
                .Register<NumberOfProductsInBasketExecutor>(c => new NumberOfProductsInBasketExecutor(
                    c.Resolve<IStatefulRepository<NumberOfProductsInBasket>>()
                ))
                .As<IExecuteQueries>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
