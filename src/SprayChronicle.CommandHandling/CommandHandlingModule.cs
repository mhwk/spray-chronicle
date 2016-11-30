using Autofac;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.CommandHandling
{
    public class CommandHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<SubscriptionCommandBus>(c => new SubscriptionCommandBus())
                .SingleInstance();
            
            builder
                .Register<LoggingCommandBus>(c => new LoggingCommandBus(
                    new LoggerFactory().CreateLogger("CommandHandling"),
                    c.Resolve<SubscriptionCommandBus>()
                ))
                .SingleInstance();
            
            builder
                .Register<ThreadedCommandBus>(c => new ThreadedCommandBus(
                    c.Resolve<LoggingCommandBus>()
                ))
                .SingleInstance();
        }
    }
}
