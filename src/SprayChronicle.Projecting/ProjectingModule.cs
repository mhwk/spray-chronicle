using Microsoft.Extensions.Logging;
using Autofac;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Projecting
{
    public sealed class ProjectingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<ProjectorHandlerFactory>(
                    c => new ProjectorHandlerFactory(
                        c.Resolve<ILogger<IStream>>(),
                        c.Resolve<IBuildProjectors>()
                    )
                )
                .AsSelf()
                .As<IBuildProjectorHandlers>()
                .SingleInstance();
        }
    }
}