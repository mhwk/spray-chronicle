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
            server.OnShutdown += services => ((DisposableDatabaseFactory)services.GetService(typeof(DisposableDatabaseFactory))).Dispose();
            return server;
        }
    }
}
