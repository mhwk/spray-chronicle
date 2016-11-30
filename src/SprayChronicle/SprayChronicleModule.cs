using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;

namespace SprayChronicle
{
    public class SprayChronicleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CommandHandlingModule>();
            builder.RegisterModule<EventHandlingModule>();
        }
    }
}