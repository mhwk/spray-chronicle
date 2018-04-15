using System;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.Server
{
    public static class ChronicleLogging
    {
        private static Stopwatch _started;

        public static void Subscribe(ChronicleServer server)
        {
            server.OnStartup          += OnStartup;
            server.OnShutdown         += OnShutdown;
            server.OnApplicationBuild += OnApplicationBuild;
            server.OnAutofacConfigure += OnAutofacConfigure;
            server.OnServiceConfigure += OnServiceConfigure;
            server.OnWebHostBuild     += OnWebHostBuild;
        }

        private static void OnStartup(IServiceProvider services)
        {
            _started = Stopwatch.StartNew();
            
            LoggerFrom(services).LogInformation(
                "\n" +
                "                   @@@@@@#                                               ,@@@@@@\n" +
                "                 @@@@@@@@@@&                                           /@@@@@@@@@@\n" +
                "               /@@@@@@@@@@@@@                                         @@@@@@@@@@@@@/\n" +
                "              %@@@@@@@@@@@@@@@                                       @@@@@@@@@@@@@@@%\n" +
                "             @@@@@@@@@@@@@@@@@@/                                   .@@@@@@@@@@@@@@@@@@\n" +
                "            .@@@@@@@@@@@@@@@@@@@.                                  @@@@@@@@@@@@@@@@@@@,\n" +
                "            @@@@@@@@@@@@@@@@@@@@@                                 @@@@@@@@@@@@@@@@@@@@@\n" +
                "           &@@@@@@@@@@@@@@@@@@@@@@                               @@@@@@@@@@@@@@@@@@@@@@&\n" +
                "           @@@@@@@@@@@@@@@@@@@@@@@@                             %@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "           @@@@@@@@@@@@@@@@@@@@@@@@                             @@@@@@@@@@@@@@@@@@@@@@@@.\n" +
                "          #@@@@@@@@@@@@@@@@@@@@@@@@@                           @@@@@@@@@@@@@@@@@@@@@@@@@&\n" +
                "          @@@@@@@@@@@@@@@@@@@@@@@@@@&                         #@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "          @@@@@@@@@@@@@@@@@@@@@@@@@@@                         @@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "          @@@@@@@@@@@@@@@@@@@@@@@@@@@@                       %@@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "          @@@@@@@@@@@@@@@@@@@@@@@@@@@@                       @@@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "          @@@@@@@@@@@@@@@@@@@@@@@@@@@@&                     %@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "          @@@@@@@@@@@@@@@@@@@@@@@@@@@@@                     @@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "          %@@@@@#           ,@@@@@@@@@@@                   %@@@@@@@@@@#          .&@@@@@@\n" +
                "          ,@@@*                  .@@@@@@                   @@@@@@/                  .@@@/\n" +
                "           @@@&                      @@@/                 ,@@@                      %@@@\n" +
                "           @@@@                        .@                 @/                        @@@@\n" +
                "           .@@@                                                                     @@@*\n" +
                "            @@@*                                                                   .@@@\n" +
                "            @@@@                                                                   @@@@\n" +
                "            @@@@(                                                                 ,@@@@.\n" +
                "           @@@@@@(                                                               ,@@@@@@\n" +
                "          @@@@@@@@@                      %@             @%                      @@@@@@@@@\n" +
                "        (@@@@@@@@@@@@@                @@@@@&           %@@@@&                &@@@@@@@@@@@@(\n" +
                "       ,@@@@@@@@@@@@@@@@@@@@%((%@@@@@@@@@@@@           @@@@@@@@@@@%/,,*%@@@@@@@@@@@@@@@@@@@*\n" +
                "       @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@           @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "       %@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@%         /@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "        @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@*           /@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n" +
                "          (@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@               @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&\n" +
                "              &@@@@@@@@@@@@@@@@@@@@@@@@@@@               &@@@@@@@@@@@@@@@@@@@@@@@@@@@.\n" +
                "                    @@@@@@@@@@@@@@@@@@@@@@               @@@@@@@@@@@@@@@@@@@@@@\n" +
                "                      /@@@@@@@@@@@@@@@@@@@@             @@@@@@@@@@@@@@@@@@@@&\n" +
                "                       .@@@@@@@@@@@@@@@@@@@@@@.      @@@@@@@@@@@@@@@@@@@@@@/\n" +
                "                        @@@@@@@@@@@@@@@@@@@@@@&     #@@@@@@@@@@@@@@@@@@@@@@\n" +
                "                        .@@@@@@@@@@@@@@@@@@@@@@     @@@@@@@@@@@@@@@@@@@@@@(\n" +
                "                           (##@@@@@@@@@@@@@@@@@     @@@@@@@@@@@@@@@@@@@&\n" +
                "                         @@@@@  *#@@@@(@@@@@@@@.    @@@@@@@@&@@@@&%  %@@@@\n" +
                "                         #@@@@@&@/%                             *.@%@@@@@@\n" +
                "                            &@& @@@@@@@ @@@@. #@   @/  @@@@ @@@@@@@*@@@\n" +
                "                                   .@@@@@@@@@@@@.  @@@@@@@@@@@@/\n" +
                "                                         ..  &@@& (@@@  //\n" +
                "\n" +
                "                                           SprayChronicle\n" +
                "                                                MHWK\n" +
                "\n"
            );

            LoggerFrom(services).LogInformation(
                $"Environment variables:\n{string.Join("\n", ChronicleServer.Variables.Select(kv => $"  {kv.Key}={kv.Value}"))}"
            );
        }

        private static void OnShutdown(IServiceProvider services)
        {
            LoggerFrom(services).LogInformation("Shutting down after {0}ms", _started.ElapsedMilliseconds);
        }

        private static void OnApplicationBuild(IApplicationBuilder services)
        {
            
        }

        private static void OnAutofacConfigure(ContainerBuilder builder)
        {
            
        }

        private static void OnServiceConfigure(IServiceCollection services)
        {
            
        }

        private static void OnWebHostBuild(IWebHostBuilder webhost)
        {
            switch (ChronicleServer.Env("ASPNETCORE_ENVIRONMENT", "Development")) {
                default:
                    webhost.ConfigureLogging(configure => configure.SetMinimumLevel(LogLevel.Information));
                    break;
                case "Development":
                    webhost.ConfigureLogging(configure => configure.SetMinimumLevel(LogLevel.Debug));
                    break;
            }
        }

        private static Microsoft.Extensions.Logging.ILogger<ChronicleServer> LoggerFrom(IServiceProvider services)
        {
            var factory = services.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
            return factory.CreateLogger<ChronicleServer>();
        }
    }
}
