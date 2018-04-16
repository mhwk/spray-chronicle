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
                .SingleInstance();
            
            builder
                .Register(c => new LoggingRouter(
                    c.Resolve<ILoggerFactory>().Create<IRouter<IExecute>>(),
                    new MeasureMilliseconds(),
                    c.Resolve<QueryRouter>()
                ))
                .AsSelf()
                .As<IRouter<IExecute>>()
                .SingleInstance();
        }

        private static void SubscribeRoutables(IComponentContext context, QueryRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(s => s.Activator.LimitType.IsAssignableTo<IRouterSubscriber<IExecute>>())
                .Select(s => context.Resolve(s.Activator.LimitType) as IRouterSubscriber<IExecute>)
                .ToList()
                .ForEach(s => router.Subscribe(s));
        }
    }
}