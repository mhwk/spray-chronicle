using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using SprayChronicle.Server;

namespace SprayChronicle.UI.Web
{
    public static class WebUIExtensions
    {
        public static ChronicleServer WithWebUI(this ChronicleServer server)
        {
            server.OnAutofacConfigure += builder =>
            {
                builder.RegisterModule<Module>();
            };
            server.OnApplicationBuild += app =>
            {
                var builder = new RouteBuilder(app);
                
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new EmbeddedFileProvider(
                        typeof(WebUIExtensions).GetTypeInfo().Assembly,
                        "SprayChronicle.UI.Web.wwwroot.dist"
                    ),
                    RequestPath = new PathString("/_ui")
                });
                
                if (ChronicleServer.Env("ASPNETCORE_ENVIRONMENT", "Development") == "Development") {
//                    builder.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
//                    {
//                        HotModuleReplacement = true,
//                        
//                    });
                }

                var assembly = typeof(WebUIExtensions).GetTypeInfo().Assembly;
                
                builder.MapGet("_ui/{*path}", async context => {
                    var resource = assembly.GetManifestResourceStream("SprayChronicle.UI.Web.wwwroot.index.html");
                    context.Response.ContentType = "text/html";
                    context.Response.ContentLength = resource.Length;

                    var cancellationTokenSource = new CancellationTokenSource();
                    await StreamCopyOperation.CopyToAsync(resource, context.Response.Body, resource.Length, cancellationTokenSource.Token);
                });

                app.UseRouter(builder.Build());
            };

            return server;
        }
    }
}
