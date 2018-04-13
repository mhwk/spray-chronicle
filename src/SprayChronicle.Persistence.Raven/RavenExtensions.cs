using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public static class RavenExtensions
    {
        public static ContainerBuilder RegisterRavenDbPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<RavenModule>();
            return builder;
        }
        
        public static ChronicleServer WithRavenDbPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterRavenDbPersistence();
            return server;
        }
    }
}
