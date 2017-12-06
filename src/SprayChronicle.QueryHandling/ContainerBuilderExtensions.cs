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
        
        public static ContainerBuilder RegisterQueryHandler<TState,THandler>(this ContainerBuilder builder, string stream)
            where TState : class
            where THandler : QueryHandler<TState>
        {
            builder.RegisterModule(new QueryHandlingModule.QueryHandler<TState,THandler>(stream));
            return builder;
        }
    }
}
