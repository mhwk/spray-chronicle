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
            builder.RegisterCommandPipeline<HandleBasket,Basket>("$ce-SprayChronicle");
            builder.RegisterCommandPipeline<HandleOrder,Order>("$ce-SprayChronicle");
            
            builder.RegisterCommandPipeline<QueryBasketWithProducts,BasketWithProducts>("$ce-SprayChronicle");
        }
    }
}
