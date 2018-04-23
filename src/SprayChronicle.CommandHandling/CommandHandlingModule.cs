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
        }

        private static void RegisterCommandHandlers(IComponentContext context, CommandRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IMessagingStrategyRouterSubscriber<IHandle>>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IMessagingStrategyRouterSubscriber<IHandle>)
                .ToList()
                .ForEach(handler => router.Subscribe(handler));
        }

        public sealed class CommandPipeline<THandler,TSourced> : Module
            where THandler : class, IHandle, IProcess
            where TSourced : EventSourced<TSourced>
        {
            private readonly string _streamName;
            
            public CommandPipeline()
            {
                _streamName = null;
            }

            public CommandPipeline(string streamName)
            {
                _streamName = streamName;
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
                    .As<IMessagingStrategyRouterSubscriber<IHandle>>()
                    .SingleInstance();

                
                if (null != _streamName) {
                    builder
                        .Register(c => new ProcessingPipeline<THandler>(
                            c.Resolve<ILoggerFactory>().Create<THandler>(),
                            c.Resolve<IEventSourceFactory>(),
                            new PersistentOptions(_streamName, typeof(THandler).FullName), 
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
