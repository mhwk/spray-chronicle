using System;
using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Mongo
{
    public static class MongoExtensions
    {
        public static ChronicleServer WithMongoPersistence(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterMongoPersistence();
            return server;
        }
        
        public static ContainerBuilder RegisterMongoPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<MongoModule>();
            return builder;
        }
    }
}
