using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public static class OuroExtensions
    {
        public static ChronicleServer WithOuroPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterOuroPersistence();
            return server;
        }
        
        public static ContainerBuilder RegisterOuroPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<OuroModule>();
            return builder;
        }
    }
}
