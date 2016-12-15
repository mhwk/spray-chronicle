using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.Example.Coordination;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.Example
{
    public class ExampleCoordinationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CommandHandlingModule.OverloadHandler<BasketHandler,Basket>>();
        }
    }
}
