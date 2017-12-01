using Autofac;
using Autofac.Core;
using Microsoft.AspNetCore.Hosting;
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
    }
}