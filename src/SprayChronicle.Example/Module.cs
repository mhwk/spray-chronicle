using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Application.State;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Example
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterCommandHandler<Basket, HandleBasket>("$ce-SprayChronicle");
            builder.RegisterCommandHandler<Order, HandleOrder>("$ce-SprayChronicle");
            
            builder.RegisterQueryHandler<BasketWithProducts, QueryBasketWithProducts>("$ce-SprayChronicle");
            builder.RegisterQueryHandler<PickedUpBasketsPerDay, QueryPickedUpBasketsPerDay>("$ce-SprayChronicle");
        }
    }
}
