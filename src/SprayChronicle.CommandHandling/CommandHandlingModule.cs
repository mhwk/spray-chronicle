using System.Linq;
using Autofac;
using Autofac.Core;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

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
                .Register(c => new LoggingDispatcher(
                    c.Resolve<ILoggerFactory>().Create<IDispatchCommands>(),
                    new MeasureMilliseconds(),
                    c.Resolve<SubscriptionDispatcher>()
                ))
                .AsSelf()
                .SingleInstance();
            
            builder
                .Register(c => new ErrorSuppressingDispatcher(
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
            where THandler : IHandleCommands, IProcessEvents
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
