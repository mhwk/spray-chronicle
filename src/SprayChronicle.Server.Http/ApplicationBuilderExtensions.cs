using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SprayChronicle.Server.Http
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseChronicleRouting(this IApplicationBuilder app)
        {
            var builder = new RouteBuilder(app);
            ((HttpCommandRouteMapper)app.ApplicationServices.GetService(typeof(HttpCommandRouteMapper))).Map(builder);
            ((HttpQueryRouteMapper)app.ApplicationServices.GetService(typeof(HttpQueryRouteMapper))).Map(builder);
            app.UseRouter(builder.Build());
        }
    }
}
