using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SprayChronicle.Server;

namespace SprayChronicle.UI.Web
{
    public static class WebUIExtensions
    {
        public static ChronicleServer WithWebUI(this ChronicleServer server)
        {
            server.OnApplicationBuild += builder =>
            {
                builder.UseStaticFiles(new StaticFileOptions
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
            };

            return server;
        }
    }
}
