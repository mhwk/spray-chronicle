using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.Server;
using SprayChronicle.Server.HealthChecks;
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
                .WithHealthChecks()
//                .WithOuroPersistence()
//                .WithMongoPersistence()
                .WithModule<Module>()
                .Run(args);
        }
    }
}
