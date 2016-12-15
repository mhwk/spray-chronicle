using Autofac;
using SprayChronicle.EventHandling;
using SprayChronicle.Server;
using SprayChronicle.Projecting;

namespace SprayChronicle.Persistence.Mongo
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithMongoPersistence(this SprayChronicleServer server)
        {
            server.OnConfigure += builder => builder.RegisterModule<EventHandlingModule>();
            server.OnConfigure += builder => builder.RegisterModule<MongoModule>();
            server.OnConfigure += builder => builder.RegisterModule<ProjectingModule>();
            return server;
        }
    }
}
