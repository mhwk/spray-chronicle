using System.Linq;
using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.CommandHandling
{
    public class CommandHandlingModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<SubscriptionCommandBus>(c => new SubscriptionCommandBus())
                .OnActivating(e => RegisterCommandHandlers(e.Context, e.Instance as SubscriptionCommandBus))
                .SingleInstance();
            
            builder
                .Register<LoggingCommandBus>(c => new LoggingCommandBus(
                    c.Resolve<ILoggerFactory>().CreateLogger<LoggingCommandBus>(),
                    c.Resolve<SubscriptionCommandBus>()
                ))
                .SingleInstance();
            
            builder
                .Register<ErrorSuppressingCommandBus>(c => new ErrorSuppressingCommandBus(
                    c.Resolve<LoggingCommandBus>()
                ))
                .SingleInstance();
            
            builder
                .Register<ThreadedCommandBus>(c => new ThreadedCommandBus(
                    c.Resolve<ErrorSuppressingCommandBus>()
                ))
                .SingleInstance();
        }

        void RegisterCommandHandlers(IComponentContext context, SubscriptionCommandBus dispatcher)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleCommand>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleCommand)
                .ToList()
                .ForEach(h => dispatcher.Subscribe(h));
        }

        public class OverloadHandler<THandler,TSourced> : Autofac.Module where THandler : OverloadCommandHandler<TSourced> where TSourced : EventSourced<TSourced>
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .Register<IEventSourcingRepository<TSourced>>(c => new EventSourcedRepository<TSourced>(
                        c.Resolve<IEventStore>()
                    ))
                    .SingleInstance();
                
                builder
                    .RegisterType<THandler>()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                    .As<IHandleCommand>()
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}
