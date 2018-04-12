using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public static class ChronicleServerExtensions
    {
        public static ChronicleServer WithRavenDbPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterRavenDbPersistence();
            return server;
        }
    }
}
