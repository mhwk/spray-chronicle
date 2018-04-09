using System;
using System.IO;
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
                throw new NullReferenceException("HttpCommandRouteMapper not found in services - is ChronicleServerHttpModule registered to Autofac?");
            }
            if (null == queryMapper) {
                throw new NullReferenceException("HttpQueryRouteMapper not found in services - is ChronicleServerHttpModule registered to Autofac?");
            }

            var builder = new RouteBuilder(app);
            
            commandMapper.Map(builder);
            queryMapper.Map(builder);
            
            app.UseRouter(builder.Build());
        }
    }
}
