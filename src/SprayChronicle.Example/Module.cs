using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.Example.Application.Effect;
using SprayChronicle.Example.Application.Model;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CommandHandlingModule.OverloadHandler<BasketHandler,Basket>>();
            builder.RegisterModule<CommandHandlingModule.OverloadHandler<OrderHandler,Order>>();
            
            builder.RegisterModule(new ProjectingModule.ProjectionWithQuery<NumberOfProductsInBasket,NumberOfProductsInBasketProjector,NumberOfProductsInBasketExecutor>("$ce-SprayChronicle"));
            builder.RegisterModule(new ProjectingModule.ProjectionWithQuery<PickedUpBasketsPerDay,PickedUpBasketsPerDayProjector,PickedUpBasketsPerDayExecutor>("$ce-SprayChronicle"));
            builder.RegisterModule(new EventHandlingModule.Persistent<OrderReceptor>("$ce-SprayChronicle"));
        }
    }
}
