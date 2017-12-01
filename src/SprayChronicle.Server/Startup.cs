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
        private readonly ChronicleServer _server;

        public Startup(ChronicleServer server)
        {
            _server = server;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            
            _server.OnServiceConfigure?.Invoke(services);
            _server.OnAutofacConfigure?.Invoke(builder);

            builder.Populate(services);
            builder.RegisterModule<ChronicleServerModule>();
            
            return new AutofacServiceProvider(builder.Build());
        }

        public void Configure(IApplicationBuilder app)
        {
            _server.OnApplicationBuild?.Invoke(app);
        }
    }
}
