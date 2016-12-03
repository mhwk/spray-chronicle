using SprayChronicle.Server;
using SprayChronicle.Server.Http;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Mongo;
using SprayChronicle.Persistence.Ouro;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new SprayChronicleServer()
                .WithEventHandling()
                .WithOuroPersistence()
                .WithMongoPersistence()
                .WithHttp()
                .RunAsync()
                .Wait();
        }
    }
}
