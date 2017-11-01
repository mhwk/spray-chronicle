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
                .Register<SubscriptionDispatcher>(c => new SubscriptionDispatcher())
                .AsSelf()
                .OnActivating(e => RegisterCommandHandlers(e.Context, e.Instance as SubscriptionDispatcher))
                .SingleInstance();
            
            builder
                .Register<LoggingDispatcher>(c => new LoggingDispatcher(
                    c.Resolve<ILoggerFactory>().CreateLogger<LoggingDispatcher>(),
                    c.Resolve<SubscriptionDispatcher>()
                ))
                .AsSelf()
                .SingleInstance();
            
            builder
                .Register<ErrorSuppressingDispatcher>(c => new ErrorSuppressingDispatcher(
                    c.Resolve<LoggingDispatcher>()
                ))
                .AsSelf()
                .SingleInstance();
            
            builder
                .Register<ThreadedDispatcher>(c => new ThreadedDispatcher(
                    c.Resolve<ErrorSuppressingDispatcher>()
                ))
                .AsSelf()
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
