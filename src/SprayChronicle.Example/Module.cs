using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Example.Domain.State;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Example
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterCommandHandler<Basket, BasketCommandHandler>("$ce-SprayChronicle");
            builder.RegisterCommandHandler<Order, OrderCommandHandler>("$ce-SprayChronicle");
            
            builder.RegisterQueryHandler<NumberOfProductsInBasket, NumberOfProductsInBasketQueryHandler>("$ce-SprayChronicle");
            builder.RegisterQueryHandler<PickedUpBasketsPerDay, PickedUpBasketsPerDayQueryHandler>("$ce-SprayChronicle");
        }
    }
}
