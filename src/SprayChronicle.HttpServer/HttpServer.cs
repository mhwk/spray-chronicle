using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SprayChronicle.HttpServer
{
    public class HttpServer
    {
        Action<ContainerBuilder> _configureContainer;

        public IContainer Container {get; private set;}

        public HttpServer(Action<ContainerBuilder> configureContainer)
        {
            _configureContainer = configureContainer;
        }

        public void Run()
        {
            _configureContainer(Startup.ContainerBuilder);

            var server = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
            

            // ((IManageStreamHandlers)server.Services.GetService(typeof(IManageStreamHandlers))).Manage();
            // ((Simulator)server.Services.GetService(typeof(Simulator))).RunAsync();

            server.Run();
        }

        public class Startup
        {
            public static ContainerBuilder ContainerBuilder = new ContainerBuilder();

            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddLogging();
                services.AddRouting();
                services.AddCors(options => {
                    options.AddPolicy("CorsPolicy", policy =>
                        policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials()
                            .AllowAnyOrigin()
                    );
                });

                ContainerBuilder.Populate(services);
                return new AutofacServiceProvider(ContainerBuilder.Build());
            }

            public void Configure(IApplicationBuilder app)
            {
                var builder = new RouteBuilder(app);

                ((HttpCommandRouteMapper)app.ApplicationServices.GetService(typeof(HttpCommandRouteMapper))).Map(builder);

                app.UseRouter(builder.Build());
            }
        }
    }
}