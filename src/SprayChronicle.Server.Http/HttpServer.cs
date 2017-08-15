using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                .UseUrls("http://0.0.0.0:5000/")
                .UseStartup<Startup>()
                .ConfigureLogging(builder => Console.WriteLine("Configure logging"))
                .Build();
        }

        public void Run()
        {
            if (null == server) {
                throw new NullReferenceException("Server not yet configured");
            }

            server.Start();
        }

        public class Startup
        {
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddCors();
                services.AddRouting();

                SprayChronicleServer.ContainerBuilder().Populate(services);
                return SprayChronicleServer.Container().Resolve<IServiceProvider>();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseCors(policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowCredentials()
                );

                var builder = new RouteBuilder(app);
                ((HttpCommandRouteMapper)app.ApplicationServices.GetService(typeof(HttpCommandRouteMapper))).Map(builder);
                ((HttpQueryRouteMapper)app.ApplicationServices.GetService(typeof(HttpQueryRouteMapper))).Map(builder);
                app.UseRouter(builder.Build());
            }
        }
    }
}
