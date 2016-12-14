using Autofac;
using SprayChronicle.Projecting;
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
            builder.RegisterModule(new ProjectingModule.ProjectionWithQuery<NumberOfProductsInBasket,NumberOfProductsInBasketProjector,NumberOfProductsInBasketExecutor>(
                "$ce-SprayChronicle",
                "SprayChronicle.Example.Contracts.Events"
            ));
        }
    }
}
