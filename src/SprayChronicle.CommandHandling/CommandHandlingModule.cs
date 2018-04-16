using System.Linq;
using Autofac;
using Autofac.Core;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.CommandHandling
{
    public class CommandHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CommandRouter>()
                .AsSelf()
                .OnActivating(e => RegisterCommandHandlers(e.Context, e.Instance))
                .SingleInstance();
            
            builder
                .Register(c => new LoggingRouter(
                    c.Resolve<ILoggerFactory>().Create<IRouter<IHandle>>(),
                    new MeasureMilliseconds(),
                    c.Resolve<CommandRouter>()
                ))
                .AsSelf()
                .As<IRouter<IHandle>>()
                .SingleInstance();
            
            builder
                .Register(c => new ErrorSuppressingRouter(
                    c.Resolve<LoggingRouter>()
                ))
                .AsSelf()
                .SingleInstance();
        }

        private static void RegisterCommandHandlers(IComponentContext context, CommandRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IRouterSubscriber<IHandle>>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IRouterSubscriber<IHandle>)
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
                        reg => reg.IsRegistered(new TypedService(typeof(CommandRouter)))
                    );
                }
            }
        }
    }
}
