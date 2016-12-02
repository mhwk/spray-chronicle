using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.HttpServer
{
    public class SprayChronicleHttpModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<HttpCommandRouteMapper>(c => new HttpCommandRouteMapper(
                c.Resolve<ILogger<HttpCommandDispatcher>>(),
                c.Resolve<SubscriptionCommandBus>()
            ));

            builder
                .Register<ILogger<HttpCommandDispatcher>>(
                    c => new LoggerFactory()
                        .AddDebug()
                        .AddConsole()
                        .CreateLogger<HttpCommandDispatcher>()
                );
        }
    }
}
