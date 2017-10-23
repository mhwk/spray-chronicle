using SprayChronicle.Server;

namespace SprayChronicle.EventHandling
{
    public static class ChronicleServerExtensions
    {
        public static ChronicleServer WithEventHandling(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterEventHandling();
            server.OnStartup += services => ((IManageStreamHandlers)services.GetService(typeof(IManageStreamHandlers))).Manage();
            return server;
        }
    }
}