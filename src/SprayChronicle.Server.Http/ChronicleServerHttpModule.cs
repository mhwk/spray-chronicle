using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.QueryHandling;

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
                    c.Resolve<CommandRouter>()
                ))
                .SingleInstance();

            builder
                .Register(c => new HttpQueryRouteMapper(
                    c.Resolve<ILoggerFactory>().Create<HttpQueryProcessor>(),
                    c.Resolve<IValidator>(),
                    c.Resolve<QueryHandling.QueryRouter>()
                ))
                .SingleInstance();
            
            builder.Register<IValidator>(c => new AnnotationValidator()).SingleInstance();
        }
    }
}
