using Autofac;
using SprayChronicle.EventHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Memory
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithMemoryPersistence(this SprayChronicleServer server)
        {
            server.OnConfigure += builder => builder.RegisterModule<AsyncEventHandlingModule>();
            server.OnConfigure += builder => builder.RegisterModule<MemoryModule>();
            return server;
        }
    }
}
