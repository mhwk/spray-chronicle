using System;
using SprayChronicle.Server;
using SprayChronicle.Server.Http;
using SprayChronicle.EventHandling;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new SprayChronicleServer()
                .WithContainerConfiguration(builder => {})
                .WithEventHandling()
                .WithHttp()
                .RunAsync()
                .Wait();
        }
    }
}
