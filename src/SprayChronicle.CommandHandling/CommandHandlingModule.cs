using System.Linq;
using Autofac;
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
                .Register<ICommandDispatcher>(c => new RouterCommandDispatcher(c.Resolve<CommandRouter>()))
                .AsSelf()
                .SingleInstance();
        }

        private static void RegisterCommandHandlers(IComponentContext context, CommandRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IMailStrategyRouterSubscriber<IHandle>>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IMailStrategyRouterSubscriber<IHandle>)
                .ToList()
                .ForEach(handler => router.Subscribe(handler));
        }

        public sealed class CommandPipeline<THandler,TSourced> : Module
            where THandler : class, IHandle, IProcess
            where TSourced : EventSourced<TSourced>
        {
            private readonly StreamOptions _streamOptions;
            
            public CommandPipeline(string streamName = null)
            {
                if (null != streamName) {
                    _streamOptions = new StreamOptions(streamName);
                }
            }
            
            public CommandPipeline(StreamOptions streamOptions)
            {
                _streamOptions = streamOptions;
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
                    .AsSelf()
                    .SingleInstance();

                builder
                    .Register(c => new HandlingPipeline<THandler, TSourced>(
                        c.Resolve<IEventSourcingRepository<TSourced>>(),
                        c.Resolve<THandler>()
                    ))
                    .AsSelf()
                    .As<IPipeline>()
                    .As<IMailStrategyRouterSubscriber<IHandle>>()
                    .SingleInstance();

                
                if (null != _streamOptions) {
                    builder
                        .Register(c => new ProcessingPipeline<THandler>(
                            c.Resolve<ILoggerFactory>().Create<THandler>(),
                            c.Resolve<IEventSourceFactory>(),
                            new CatchUpOptions(_streamOptions), 
                            c.Resolve<CommandRouter>(),
                            c.Resolve<THandler>()))
                        .AsSelf()
                        .As<IPipeline>()
                        .SingleInstance();
                }
            }
        }
    }
}
