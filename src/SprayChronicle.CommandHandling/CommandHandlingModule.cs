using System.Linq;
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
                .OnActivating(e => RegisterCommandHandlers(e.Context, e.Instance as SubscriptionCommandBus))
                .SingleInstance();
            
            builder
                .Register<LoggingCommandBus>(c => new LoggingCommandBus(
                    c.Resolve<ILogger<LoggingCommandBus>>(),
                    c.Resolve<SubscriptionCommandBus>()
                ))
                .SingleInstance();
            
            builder
                .Register<ThreadedCommandBus>(c => new ThreadedCommandBus(
                    c.Resolve<LoggingCommandBus>()
                ))
                .SingleInstance();

            builder
                .Register<ILogger<LoggingCommandBus>>(
                    c => new LoggerFactory()
                        .AddDebug()
                        .AddConsole()
                        .CreateLogger<LoggingCommandBus>()
                );
        }

        void RegisterCommandHandlers(IComponentContext context, SubscriptionCommandBus dispatcher)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleCommand>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleCommand)
                .ToList()
                .ForEach(h => dispatcher.Subscribe(h));
        }
    }
}
