using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace SprayChronicle.Server
{
    public sealed class Startup : IStartup
    {
        readonly ChronicleServer _server;

        public Startup(ChronicleServer server)
        {
            _server = server;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (null != _server.OnServiceConfigure) {
                _server.OnServiceConfigure(services);
            }

            var builder = new ContainerBuilder();

            if (null != _server.OnAutofacConfigure) {
                _server.OnAutofacConfigure(builder);
            }

            builder.Populate(services);
            builder.RegisterModule<ChronicleServerModule>();
            return new AutofacServiceProvider(builder.Build());
        }

        public void Configure(IApplicationBuilder app)
        {
            if (null != _server.OnApplicationBuild) {
                _server.OnApplicationBuild(app);
            }
        }
    }
}
