using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public static class RavenExtensions
    {
        public static ContainerBuilder RegisterQueryExecutor<TProcessor,TState>(
            this ContainerBuilder builder,
            string streamName = null,
            string checkpointName = null)
            where TProcessor : RavenQueries<TProcessor,TState>
            where TState : class
        {
            builder.RegisterModule(new RavenModule.QueryPipeline<TProcessor,TState>(streamName, checkpointName));
            return builder;
        }
        
        public static ChronicleServer WithRavenPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<RavenModule>();
            return server;
        }
    }
}
