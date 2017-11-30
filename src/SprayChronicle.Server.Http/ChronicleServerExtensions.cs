using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SprayChronicle.Server.Http
{
    public static class ChronicleServerExtensions
    {
        public static ChronicleServer WithHttp(this ChronicleServer server)
        {
            server.OnAutofacConfigure += ConfigureAutofac;
            server.OnServiceConfigure += ConfigureService;
            server.OnApplicationBuild += BuildApplication;
            server.OnWebHostBuild += BuildWebHost;
            return server;
        }

        private static void ConfigureAutofac(ContainerBuilder builder)
        {
            builder.RegisterChronicleHttp();
        }

        private static void ConfigureService(IServiceCollection services)
        {
            services.AddCors();
            services.AddRouting();
        }

        private static void BuildApplication(IApplicationBuilder app)
        {
            app.UseCors(policy => policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .AllowCredentials()
            );
            app.UseChronicleRouting();
        }

        private static void BuildWebHost(IWebHostBuilder webHost)
        {
            webHost.UseKestrel();
            webHost.UseUrls("http://0.0.0.0:5000/");
        }
    }
}
