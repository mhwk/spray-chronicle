using Autofac;
using SprayChronicle.EventHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Mongo
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithMongoPersistence(this SprayChronicleServer server)
        {
            server.OnConfigure += builder => builder.RegisterModule<EventHandlingModule>();
            server.OnConfigure += builder => builder.RegisterModule<MongoModule>();
            return server;
        }
    }
}
