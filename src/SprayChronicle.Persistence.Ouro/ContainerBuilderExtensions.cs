using Autofac;

namespace SprayChronicle.Persistence.Ouro
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterOuroPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<OuroModule>();
            return builder;
        }
    }
}
