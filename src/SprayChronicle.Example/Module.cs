using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Application.State;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Persistence.Raven;

namespace SprayChronicle.Example
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterCommandHandler<HandleBasket,Basket>("$ce-SprayChronicle");
            builder.RegisterCommandHandler<HandleOrder,Order>("$ce-SprayChronicle");
            
            builder.RegisterQueryExecutor<QueryBasketWithProducts,BasketWithProducts_v1>("$ce-SprayChronicle");
        }
    }
}
