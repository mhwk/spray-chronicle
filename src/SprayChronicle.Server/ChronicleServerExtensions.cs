using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.Server
{
    public static class ChronicleServerExtensions
    {
        public static ChronicleServer WithModule<T>(this ChronicleServer server) where T : IModule, new()
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<T>();
            return server;
        }

        public static ChronicleServer WithLifetimeInfo(this ChronicleServer server)
        {
            server.OnStartup += ChronicleLogging.OnStartup;
            server.OnShutdown += ChronicleLogging.OnShutdown;
            server.OnApplicationBuild += ChronicleLogging.OnApplicationBuild;
            server.OnAutofacConfigure += ChronicleLogging.OnAutofacConfigure;
            server.OnServiceConfigure += ChronicleLogging.OnServiceConfigure;
            server.OnWebHostBuild += ChronicleLogging.OnWebHostBuild;
            return server;
        }
    }
}