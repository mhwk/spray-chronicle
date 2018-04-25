using System.Linq;
using Autofac;

namespace SprayChronicle.EventHandling
{
    public sealed class EventHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<PipelineManager>()
                .OnActivating(e => RegisterPipelines(e.Context, e.Instance))
                .AsSelf()
                .As<IPipelineManager>()
                .SingleInstance();
        }

        private static void RegisterPipelines(IComponentContext context, IPipelineManager manager)
        {
            context.ComponentRegistry.Registrations
                .Where(pipeline => pipeline.Activator.LimitType.IsAssignableTo<IPipeline>())
                .Select(pipeline => context.Resolve(pipeline.Activator.LimitType) as IPipeline)
                .ToList()
                .ForEach(pipeline => manager.Attach(pipeline));
        }
    }
}
