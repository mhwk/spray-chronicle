using System.Linq;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.CommandHandling
{
    public class CommandHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SubscriptionDispatcher>()
                .AsSelf()
                .As<IDispatchCommands>()
                .OnActivating(e => RegisterCommandHandlers(e.Context, e.Instance))
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
        }

        private static void RegisterCommandHandlers(IComponentContext context, SubscriptionDispatcher dispatcher)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleCommands>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleCommands)
                .ToList()
                .ForEach(handler => dispatcher.Subscribe(handler));
        }

        public class CommandHandler<TSourced,THandler> : Module
            where TSourced : EventSourced<TSourced>
            where THandler : IHandleCommands, IHandleEvents
        {
            private readonly string _stream;
            
            public CommandHandler()
            {
                _stream = null;
            }

            public CommandHandler(string stream)
            {
                _stream = stream;
            }

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
                    .As<IHandleCommands>()
                    .AsSelf()
                    .SingleInstance();

                
                if (null != _stream) {
                    builder.RegisterEventHandler<THandler>(
                        _stream,
                        reg => reg.IsRegistered(new TypedService(typeof(SubscriptionDispatcher)))
                    );
                }
            }
        }
    }
}
