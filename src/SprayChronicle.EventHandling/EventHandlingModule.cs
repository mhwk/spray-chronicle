using System.Linq;
using Microsoft.Extensions.Logging;
using Autofac;

namespace SprayChronicle.EventHandling
{
    public sealed class EventHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<IManageStreamHandlers>(c => new StreamHandlerManager())
                .OnActivating(e => RegisterStreamHandlers(e.Context, e.Instance as IManageStreamHandlers))
                .SingleInstance();

            builder
                .Register<ILogger<StreamEventHandler>>(
                    c => new LoggerFactory()
                        .AddDebug()
                        .AddConsole()
                        .CreateLogger<StreamEventHandler>()
                )
                .SingleInstance();
        }

        void RegisterStreamHandlers(IComponentContext context, IManageStreamHandlers manager)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleStream>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleStream)
                .ToList()
                .ForEach(h => manager.Add(h));
        }
    }
}