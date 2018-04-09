using System;
using System.IO;
using App.Metrics.Health;
using App.Metrics.Health.Builder;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SprayChronicle.Server.HealthChecks
{
    public static class ChronicleServerExtensions
    {
        public static ChronicleServer WithHealthChecks(this ChronicleServer server)
        {
            return WithHealthChecks(server, additionalChecks => { });
        }

        public static ChronicleServer WithHealthChecks(this ChronicleServer server, Action<HealthCheckBuilder> additionalChecks)
        {
            server.OnAutofacConfigure += builder => builder.RegisterModule<HealthCheckModule>();
            
            server.OnApplicationBuild += app =>
            {
                var builder = new RouteBuilder(app);

                if (!(app.ApplicationServices.GetService(typeof(IHealthRoot)) is IHealthRoot health)) {
                    throw new Exception("Unable to retrieve IHealthRoot from services");
                }

                builder.MapGet("_health", async context => {
                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        var result = await health.HealthCheckRunner.ReadAsync();
                        
//                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode =  HealthCheckStatus.Healthy != result.Status ? 500 : 200;
                        
                        await health.DefaultOutputHealthFormatter.WriteAsync(context.Response.Body, result);
                    }
                });

                app.UseRouter(builder.Build());
            };
            
            return server;
        }
    }
}
