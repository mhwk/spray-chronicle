using Autofac;

namespace SprayChronicle.QueryHandling
{
    public static class QueryHandlingExtensions
    {
        public static ContainerBuilder RegisterQueryHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<QueryHandlingModule>();
            return builder;
        }
    }
}
