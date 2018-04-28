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
                .Register(c => new RouterQueryDispatcher(c.Resolve<QueryRouter>()))
                .AsSelf()
                .As<IQueryDispatcher>()
                .SingleInstance();
        }

        private static void SubscribeRoutables(IComponentContext context, QueryRouter router)
        {
            context.ComponentRegistry.Registrations
                .Where(s => s.Activator.LimitType.IsAssignableTo<IMailStrategyRouterSubscriber<IExecute>>())
                .Select(s => context.Resolve(s.Activator.LimitType) as IMailStrategyRouterSubscriber<IExecute>)
                .ToList()
                .ForEach(s => router.Subscribe(s));
        }
    }
}