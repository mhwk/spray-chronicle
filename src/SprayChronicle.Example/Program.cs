using SprayChronicle.EventHandling;
using SprayChronicle.Server;
using SprayChronicle.Server.Http;

namespace SprayChronicle.Example
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            new ChronicleServer()
                .WithEventHandling()
                .WithHttp()
//                .WithOuroPersistence()
//                .WithMongoPersistence()
                .WithModule<Module>()
                .Run();
        }
    }
}
