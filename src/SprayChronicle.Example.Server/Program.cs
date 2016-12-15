using Microsoft.Extensions.Logging;
using SprayChronicle.Server;
using SprayChronicle.Server.Http;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.Persistence.Mongo;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.Example;

namespace SprayChronicle.Example.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new SprayChronicleServer()
                #if DEBUG
                .WithLogLevel(LogLevel.Debug)
                .WithMemoryPersistence()
                #else
                .WithMongoPersistence()
                .WithOuroPersistence()
                #endif
                .WithExample()
                .WithHttp()
                .WithEventHandling()
                .Run();
        }
    }
}
