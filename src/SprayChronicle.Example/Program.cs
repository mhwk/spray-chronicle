using System.Threading.Tasks;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.Persistence.Raven;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;
using SprayChronicle.Server.HealthChecks;
using SprayChronicle.Server.Http;

namespace SprayChronicle.Example
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            await new ChronicleServer()
                .WithHealthChecks()
                .WithOuroPersistence()
//                .WithMongoPersistence()
//                .WithRavenPersistence()
                .WithEventHandling()
                .WithCommandHandling()
                .WithQueryHandling()
                .WithHttp()
                .WithExample()
                .Run(args);
        }
    }
}
