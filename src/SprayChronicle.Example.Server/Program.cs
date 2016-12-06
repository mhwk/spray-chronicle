using SprayChronicle.Server;
using SprayChronicle.Server.Http;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.Persistence.Mongo;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.Example;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new SprayChronicleServer()
                .WithMemoryPersistence()
                // .WithMongoPersistence()
                .WithOuroPersistence()
                .WithExample()
                .WithHttp()
                .WithEventHandling()
                .Run();
        }
    }
}
