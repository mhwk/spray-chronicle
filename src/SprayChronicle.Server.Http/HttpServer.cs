using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SprayChronicle.Server;

namespace SprayChronicle.Server.Http
{
    public class HttpServer
    {
        IWebHost server;

        public void Initialize()
        {
            server = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
        }

        public void Run()
        {
            if (null == server) {
                throw new NullReferenceException("Server not yet configured");
            }

            server.Run();
        }

        public class Startup
        {
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

                SprayChronicleServer.ContainerBuilder().Populate(services);
                SprayChronicleServer.ContainerBuilder().RegisterModule(new SprayChronicleHttpModule());
                return SprayChronicleServer.Container().Resolve<IServiceProvider>();
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