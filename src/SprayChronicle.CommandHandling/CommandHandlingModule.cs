using System.Linq;
using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.CommandHandling
{
    public class CommandHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new SubscriptionDispatcher())
                .OnActivating(e => RegisterCommandHandlers(e.Context, e.Instance as SubscriptionDispatcher))
                .SingleInstance();
            
            builder
                .Register(c => new LoggingDispatcher(
                    c.Resolve<ILoggerFactory>().CreateLogger<LoggingDispatcher>(),
                    c.Resolve<SubscriptionDispatcher>()
                ))
                .SingleInstance();
            
            builder
                .Register(c => new ErrorSuppressingDispatcher(
                    c.Resolve<LoggingDispatcher>()
                ))
                .SingleInstance();
            
            builder
                .Register(c => new ThreadedDispatcher(
                    c.Resolve<ErrorSuppressingDispatcher>()
                ))
                .SingleInstance();
        }

        private static void RegisterCommandHandlers(IComponentContext context, SubscriptionDispatcher dispatcher)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleCommand>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleCommand)
                .ToList()
                .ForEach(dispatcher.Subscribe);
        }

        public class OverloadHandler<THandler,TSourced> : Module where THandler : OverloadCommandHandler<TSourced> where TSourced : EventSourced<TSourced>
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
