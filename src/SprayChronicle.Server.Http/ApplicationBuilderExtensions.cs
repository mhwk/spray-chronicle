using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SprayChronicle.Server.Http
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseChronicleRouting(this IApplicationBuilder app)
        {
            var commandMapper = (HttpCommandRouteMapper)app.ApplicationServices.GetService(typeof(HttpCommandRouteMapper));
            var queryMapper = (HttpQueryRouteMapper)app.ApplicationServices.GetService(typeof(HttpQueryRouteMapper));

            if (null == commandMapper) {
                throw new NullReferenceException(string.Format("No command mapper registered - is module properly loaded?"));
            }
            if (null == queryMapper) {
                throw new NullReferenceException(string.Format("No query mapper registered - is module properly loaded?"));
            }

            var builder = new RouteBuilder(app);
            commandMapper.Map(builder);
            queryMapper.Map(builder);
            app.UseRouter(builder.Build());
        }
    }
}
