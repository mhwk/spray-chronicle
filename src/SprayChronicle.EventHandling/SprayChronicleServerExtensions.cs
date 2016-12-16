using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.EventHandling
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithEventHandling(this SprayChronicleServer server)
        {
            server.OnConfigure += builder => builder.RegisterModule<AsyncEventHandlingModule>();
            server.OnExecute += container => container.Resolve<IManageStreamHandlers>().Manage();
            return server;
        }
    }
}
