using Autofac;
using Microsoft.Extensions.DependencyInjection;
using SprayChronicle.Server;

namespace SprayChronicle.EventHandling
{
    public static class EventHandlingExtensions
    {
        public static ChronicleServer WithEventHandling(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterEventHandling();
            server.OnStartup += services => services.GetService<IPipelineManager>().Start();
            return server;
        }
        
        public static ContainerBuilder RegisterEventHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<EventHandlingModule>();
            return builder;
        }
    }
}
