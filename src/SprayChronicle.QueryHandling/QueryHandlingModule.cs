using Autofac;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.QueryHandling
{
    public class QueryHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<IBuildQueryExecutors>(c => new OverloadQueryExecutorFactory())
                .SingleInstance();
            
            builder
                .Register<SubscriptionQueryProcessor>(c => new SubscriptionQueryProcessor())
                .OnActivating(e => RegisterQueryExecutors(e.Context, e.Instance as SubscriptionQueryProcessor))
                .SingleInstance();
            
            builder
                .Register<LoggingQueryProcessor>(c => new LoggingQueryProcessor(
                    c.Resolve<ILoggerFactory>().CreateLogger<IProcessQueries>(),
                    c.Resolve<SubscriptionQueryProcessor>()
                ))
                .SingleInstance();
        }

        void RegisterQueryExecutors(IComponentContext context, SubscriptionQueryProcessor processor)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IExecuteQueries>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IExecuteQueries)
                .ToList()
                .ForEach(h => processor.AddExecutors(h));
        }
    }
}