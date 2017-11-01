using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Mongo;
using SprayChronicle.Projecting;
using SprayChronicle.Example.Application.Effect;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain.Effect;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example
{
    public class ExampleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CommandHandlingModule.OverloadHandler<BasketHandler,Basket>>();
            builder.RegisterModule(new ProjectingModule.ProjectionWithQuery<NumberOfProductsInBasket,NumberOfProductsInBasketProjector,NumberOfProductsInBasketExecutor>(
                "$ce-SprayChronicle",
                "SprayChronicle.Example.Domain"
            ));
            builder.RegisterModule(new ProjectingModule.ProjectionWithQuery<PickedUpBasketsPerDay,PickedUpBasketsPerDayProjector,PickedUpBasketsPerDayExecutor>(
                "$ce-SprayChronicle",
                "SprayChronicle.Example.Domain"
            ));
            builder.RegisterModule(new EventHandlingModule.Persistent<OrderReceptor>(
                "$ce-SprayChronicle",
                "SprayChronicle.Example.Domain"
            ));
        }
    }
}
