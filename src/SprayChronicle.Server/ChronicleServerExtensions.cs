using Autofac;
using Autofac.Core;

namespace SprayChronicle.Server
{
    public static class ChronicleServerExtensions
    {
        public static ChronicleServer WithModule<T>(this ChronicleServer server) where T : IModule, new()
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<T>();
            return server;
        }
    }
}