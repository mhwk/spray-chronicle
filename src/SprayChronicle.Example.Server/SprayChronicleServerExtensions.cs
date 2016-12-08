using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.Example.Server
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithExample(this SprayChronicleServer server)
        {
            server.OnConfigure += builder => builder.RegisterModule<ExampleProjectionModule>();
            server.OnConfigure += builder => builder.RegisterModule<ExampleCoordinationModule>();
            return server;
        }
    }
}
