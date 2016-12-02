using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.CommandHandling
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithCommandHandling(this SprayChronicleServer server)
        {
            server.OnConfigure += builder => builder.RegisterModule<CommandHandlingModule>();
            return server;
        }
    }
}
