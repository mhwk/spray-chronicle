using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public class SprayChronicleHttpModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<HttpCommandRouteMapper>(c => new HttpCommandRouteMapper(
                c.Resolve<ILogger<HttpCommandDispatcher>>(),
                c.Resolve<SubscriptionCommandBus>()
            ));

            builder.Register<HttpQueryRouteMapper>(c => new HttpQueryRouteMapper(
                c.Resolve<ILogger<HttpQueryProcessor>>(),
                c.Resolve<SubscriptionQueryProcessor>()
            ));

            builder
                .Register<ILogger<HttpCommandDispatcher>>(
                    c => new LoggerFactory()
                        .AddConsole(LogLevel.Debug)
                        .CreateLogger<HttpCommandDispatcher>()
                );

            builder
                .Register<ILogger<HttpQueryProcessor>>(
                    c => new LoggerFactory()
                        .AddConsole(LogLevel.Debug)
                        .CreateLogger<HttpQueryProcessor>()
                );
        }
    }
}
