using Autofac;

namespace SprayChronicle.Persistence.Raven
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterRavenDbPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<RavenDbModule>();
            return builder;
        }
    }
}
