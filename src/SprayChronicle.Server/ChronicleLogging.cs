using System;
using System.Diagnostics;
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
        
        public static void OnStartup(IServiceProvider services)
        {
            _started = Stopwatch.StartNew();
            
            LoggerFrom(services).LogInformation(
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
                "                                                MHWK"
            );
        }

        public static void OnShutdown(IServiceProvider services)
        {
            LoggerFrom(services).LogInformation("Shutting down after {0}ms", _started.ElapsedMilliseconds);
        }

        public static void OnApplicationBuild(IApplicationBuilder services)
        {
            
        }

        public static void OnAutofacConfigure(ContainerBuilder builder)
        {
            
        }

        public static void OnServiceConfigure(IServiceCollection services)
        {
            
        }

        public static void OnWebHostBuild(IWebHostBuilder webhost)
        {
            
        }

        private static ILogger<ChronicleServer> LoggerFrom(IServiceProvider services)
        {
            var factory = services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            return factory.CreateLogger<ChronicleServer>();
        }
    }
}