using System;
using Autofac;
using SprayChronicle.EventSourcing;
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
            builder.RegisterModule(new RavenModule.QueryPipeline<TProcessor,TState>(new StreamOptions(streamName), checkpointName));
            return builder;
        }
        
        public static ContainerBuilder RegisterQueryExecutor<TProcessor,TState>(
            this ContainerBuilder builder,
            Func<StreamOptions,StreamOptions> streamFactory,
            string checkpointName = null)
            where TProcessor : RavenQueries<TProcessor,TState>
            where TState : class
        {
            builder.RegisterModule(new RavenModule.QueryPipeline<TProcessor,TState>(streamFactory(new StreamOptions(typeof(TProcessor).FullName)), checkpointName));
            return builder;
        }
        
        public static ChronicleServer WithRavenPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<RavenModule>();
            return server;
        }
    }
}
