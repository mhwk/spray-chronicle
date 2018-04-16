using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public static class QueryHandlingExtensions
    {
        public static ChronicleServer WithQueryHandling(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<QueryHandlingModule>();
            return server;
        }
    }
}
