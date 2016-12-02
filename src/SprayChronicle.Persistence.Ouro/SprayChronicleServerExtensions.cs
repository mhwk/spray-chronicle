using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithOuroPersistence(this SprayChronicleServer server)
        {
            server.OnConfigure += builder => builder.RegisterModule<OuroModule>();
            return server;
        }
    }
}
