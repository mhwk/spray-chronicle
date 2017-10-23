using Autofac;

namespace SprayChronicle.Projecting
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterProjecting(this ContainerBuilder builder)
        {
            builder.RegisterModule<ProjectingModule>();
            return builder;
        }
    }
}
