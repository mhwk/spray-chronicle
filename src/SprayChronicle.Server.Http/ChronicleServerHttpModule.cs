using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.QueryHandling;
using SubscriptionRouter = SprayChronicle.CommandHandling.SubscriptionRouter;

namespace SprayChronicle.Server.Http
{
    public class ChronicleServerHttpModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new HttpCommandRouteMapper(
                    c.Resolve<ILoggerFactory>().Create<HttpCommandDispatcher>(),
                    c.Resolve<IValidator>(),
                    c.Resolve<SubscriptionRouter>()
                ))
                .SingleInstance();

            builder
                .Register(c => new HttpQueryRouteMapper(
                    c.Resolve<ILoggerFactory>().Create<HttpQueryProcessor>(),
                    c.Resolve<IValidator>(),
                    c.Resolve<QueryHandling.SubscriptionRouter>()
                ))
                .SingleInstance();
            
            builder.Register<IValidator>(c => new AnnotationValidator()).SingleInstance();
        }
    }
}
