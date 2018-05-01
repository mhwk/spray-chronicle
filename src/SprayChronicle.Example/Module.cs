using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Persistence.Raven;

namespace SprayChronicle.Example
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterCommandHandler<HandleBasket,Basket>();
            builder.RegisterCommandHandler<HandleOrder,Order>(stream => stream.From(typeof(Basket), typeof(Order)));
            
//            builder.RegisterQueryExecutor<QueryBasketsPickedUpPerDay,QueryBasketsPickedUpPerDay.Result>();
//            builder.RegisterQueryExecutor<QueryBasketsPickedUpPerMinute,QueryBasketsPickedUpPerMinute.Result>();
//            builder.RegisterQueryExecutor<QueryBasketWithProducts,QueryBasketWithProducts.BasketWithProducts_v1>(stream => stream.From(typeof(Basket)));
//            builder.RegisterQueryExecutor<QueryPlacedOrders,QueryPlacedOrders.PlacedOrders_v2>(stream => stream.From(typeof(Basket), typeof(Order)));
//            builder.RegisterQueryExecutor<QueryPlacedOrdersPerDay,QueryPlacedOrdersPerDay.Result>();
//            builder.RegisterQueryExecutor<QueryPlacedOrdersPerMinute,QueryPlacedOrdersPerMinute.Result>();

            builder.Register(c => new Populator(c.Resolve<ICommandDispatcher>()));
        }
    }
}
