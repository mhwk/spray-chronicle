using Autofac;

namespace SprayChronicle.Persistence.Memory
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterMemoryPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<MemoryModule>();
            return builder;
        }
    }
}
