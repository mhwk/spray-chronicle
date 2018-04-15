using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public static class RavenExtensions
    {
        public static ContainerBuilder RegisterPipeline<TProcessor,TState>(
            this ContainerBuilder builder,
            string streamName)
            where TProcessor : RavenQueryProcessor<TProcessor,TState>
            where TState : class
        {
            builder.RegisterModule(new RavenModule.QueryPipeline<TProcessor,TState>(streamName));
            return builder;
        }
        
        public static ChronicleServer WithRavenPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<RavenModule>();
            return server;
        }
    }
}
