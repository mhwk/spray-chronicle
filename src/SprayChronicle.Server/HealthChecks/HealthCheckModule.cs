using System.Linq;
using App.Metrics.Health;
using Autofac;
using App.Metrics.Health.Builder;

namespace SprayChronicle.Server.HealthChecks
{
    public sealed class HealthCheckModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new HealthBuilder())
                .OnActivating(e => { RegisterChecks(e.Context, e.Instance); })
                .AsSelf()
                .As<IHealthBuilder>()
                .SingleInstance();
            
            builder
                .Register(c => c.Resolve<IHealthBuilder>().Build())
                .AsSelf()
                .As<IHealthRoot>()
                .SingleInstance();
            
            builder
                .Register(c => new HealthCheckConsoleCommand())
                .AsSelf()
                .As<IConsoleCommand>()
                .SingleInstance();
        }
        
        private static void RegisterChecks(IComponentContext context, IHealthBuilder builder)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<HealthCheck>())
                .Select(r => context.Resolve(r.Activator.LimitType) as HealthCheck)
                .ToList()
                .ForEach(handler => builder.HealthChecks.AddCheck(handler));
        }
    }
}