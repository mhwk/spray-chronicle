using Autofac;
using System.Linq;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public class QueryHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<SubscriptionRouter>()
                .OnActivating(e => SubscribeRoutables(e.Context, e.Instance))
                .SingleInstance();
            
            builder
                .Register(c => new LoggingRouter(
                    c.Resolve<ILoggerFactory>().Create<IQueryRouter>(),
                    new MeasureMilliseconds(),
                    c.Resolve<SubscriptionRouter>()
                ))
                .AsSelf()
                .As<IQueryRouter>()
                .SingleInstance();
        }

        private static void SubscribeRoutables(IComponentContext context, SubscriptionRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(s => s.Activator.LimitType.IsAssignableTo<IQueryRouterSubscriber>())
                .Select(s => context.Resolve(s.Activator.LimitType) as IQueryRouterSubscriber)
                .ToList()
                .ForEach(s => router.Subscribe(s));
        }
    }
}