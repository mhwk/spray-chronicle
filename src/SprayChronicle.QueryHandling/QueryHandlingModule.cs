using Autofac;
using System.Linq;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public class QueryHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<QueryRouter>()
                .OnActivating(e => SubscribeRoutables(e.Context, e.Instance))
                .AsSelf()
                .SingleInstance();
            
            builder
                .Register(c => new LoggingRouter(
                    c.Resolve<ILoggerFactory>().Create<IExecute>(),
                    new MeasureMilliseconds(),
                    c.Resolve<QueryRouter>()
                ))
                .AsSelf()
                .SingleInstance();
        }

        private static void SubscribeRoutables(IComponentContext context, QueryRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(s => s.Activator.LimitType.IsAssignableTo<IMessagingStrategyRouterSubscriber<IExecute>>())
                .Select(s => context.Resolve(s.Activator.LimitType) as IMessagingStrategyRouterSubscriber<IExecute>)
                .ToList()
                .ForEach(s => router.Subscribe(s));
        }
    }
}