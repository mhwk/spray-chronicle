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
                c.Resolve<ILoggerFactory>().CreateLogger<HttpCommandDispatcher>(),
                c.Resolve<SubscriptionCommandBus>()
            ));

            builder.Register<HttpQueryRouteMapper>(c => new HttpQueryRouteMapper(
                c.Resolve<ILoggerFactory>().CreateLogger<HttpQueryProcessor>(),
                c.Resolve<SubscriptionQueryProcessor>()
            ));
        }
    }
}
