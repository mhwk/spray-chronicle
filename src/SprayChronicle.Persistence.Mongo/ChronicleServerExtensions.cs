using System;
using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Mongo
{
    public static class ChronicleServerExtensions
    {
        public static ChronicleServer WithMongoPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterMongoPersistence();
            server.OnShutdown += services => Console.WriteLine(services.GetType());
            // server.OnShutdown += services => ((IContainer)services.GetService(typeof(IContainer))).Dispose();
            return server;
        }
    }
}
