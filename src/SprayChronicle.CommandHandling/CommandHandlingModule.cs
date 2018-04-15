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
                .RegisterType<SubscriptionRouter>()
                .AsSelf()
                .As<ICommandRouter>()
                .OnActivating(e => RegisterCommandHandlers(e.Context, e.Instance))
                .SingleInstance();
            
            builder
                .Register(c => new LoggingRouter(
                    c.Resolve<ILoggerFactory>().Create<ICommandRouter>(),
                    new MeasureMilliseconds(),
                    c.Resolve<SubscriptionRouter>()
                ))
                .AsSelf()
                .SingleInstance();
            
            builder
                .Register(c => new ErrorSuppressingRouter(
                    c.Resolve<LoggingRouter>()
                ))
                .AsSelf()
                .SingleInstance();
        }

        private static void RegisterCommandHandlers(IComponentContext context, SubscriptionRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleCommands>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleCommands)
                .ToList()
                .ForEach(handler => router.Subscribe(handler));
        }

        public sealed class CommandPipeline<TSourced,THandler> : Module
            where TSourced : EventSourced<TSourced>
            where THandler : IHandleCommands, IProcessEvents
        {
            private readonly string _stream;
            
            public CommandPipeline()
            {
                _stream = null;
            }

            public CommandPipeline(string stream)
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
                        reg => reg.IsRegistered(new TypedService(typeof(SubscriptionRouter)))
                    );
                }
            }
        }
    }
}
