using Autofac;
using SprayChronicle.Projecting;
using SprayChronicle.Example.Projection;

namespace SprayChronicle.Example
{
    public class ExampleProjectionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new ProjectingModule.ProjectionWithQuery<NumberOfProductsInBasket,NumberOfProductsInBasketProjector,NumberOfProductsInBasketExecutor>(
                "$ce-SprayChronicle",
                "SprayChronicle.Example.Contracts.Events"
            ));
            builder.RegisterModule(new ProjectingModule.ProjectionWithQuery<PickedUpBasketsPerDay,PickedUpBasketsPerDayProjector,PickedUpBasketsPerDayExecutor>(
                "$ce-SprayChronicle",
                "SprayChronicle.Example.Contracts.Events"
            ));
        }
    }
}
