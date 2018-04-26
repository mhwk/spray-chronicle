using Autofac;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Server;
using Microsoft.Extensions.DependencyInjection;

namespace SprayChronicle.Example
{
    public static class Extensions
    {
        public static ChronicleServer WithExample(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<Module>();
//            server.OnStartup += async services => await services.GetService<Populator>().Populate();
            return server;
        }
    }
}
