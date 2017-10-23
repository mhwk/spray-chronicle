using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public static class ChronicleServerExtensions
    {
        public static ChronicleServer WithOuroPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterOuroPersistence();
            return server;
        }
    }
}
