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
            CommandHandlingExtensions.RegisterPipeline<Basket, HandleBasket>(builder, "$ce-SprayChronicle");
            CommandHandlingExtensions.RegisterPipeline<Order, HandleOrder>(builder, "$ce-SprayChronicle");
            
            RavenExtensions.RegisterPipeline<QueryBasketWithProducts,BasketWithProducts>(builder, "$ce-SprayChronicle");
        }
    }
}
