using Autofac;

namespace SprayChronicle.QueryHandling
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterQueryHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<QueryHandlingModule>();
            return builder;
        }
    }
}
