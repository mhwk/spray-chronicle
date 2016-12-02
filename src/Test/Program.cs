using Autofac;
using SprayChronicle.Server;
using SprayChronicle.Server.Http;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Ouro;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new SprayChronicleServer()
                .WithCommandHandling()
                .WithEventHandling()
                .WithOuroPersistence()
                .WithHttp()
                .RunAsync()
                .Wait();
        }
    }
}
