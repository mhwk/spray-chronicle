using System.Linq;
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
                    c.Resolve<ICommandDispatcher>()
                ))
                .OnActivating(e => RegisterCommandProviders(e.Context, e.Instance))
                .SingleInstance();

            builder
                .Register(c => new HttpQueryRouteMapper(
                    c.Resolve<ILoggerFactory>().Create<HttpQueryProcessor>(),
                    c.Resolve<IValidator>(),
                    c.Resolve<IQueryDispatcher>()
                ))
                .OnActivating(e => RegisterQueryProviders(e.Context, e.Instance))
                .SingleInstance();

            builder
                .Register(c => new ContextAttributeProvider<HttpCommandAttribute>())
                .AsSelf()
                .As<IAttributeProvider<HttpCommandAttribute>>();
            
            builder
                .Register(c => new ContextAttributeProvider<HttpQueryAttribute>())
                .AsSelf()
                .As<IAttributeProvider<HttpQueryAttribute>>();
            
            builder.Register<IValidator>(c => new AnnotationValidator()).SingleInstance();
        }

        private static void RegisterCommandProviders(IComponentContext context, HttpCommandRouteMapper router)
        {
            context.ComponentRegistry.Registrations
                .Where(p => p.Activator.LimitType.IsAssignableTo<IAttributeProvider<HttpCommandAttribute>>())
                .Select(p => context.Resolve(p.Activator.LimitType) as IAttributeProvider<HttpCommandAttribute>)
                .ToList()
                .ForEach(router.RegisterAttributeProvider);
        }

        private static void RegisterQueryProviders(IComponentContext context, HttpQueryRouteMapper router)
        {
            context.ComponentRegistry.Registrations
                .Where(p => p.Activator.LimitType.IsAssignableTo<IAttributeProvider<HttpQueryAttribute>>())
                .Select(p => context.Resolve(p.Activator.LimitType) as IAttributeProvider<HttpQueryAttribute>)
                .ToList()
                .ForEach(router.RegisterAttributeProvider);
        }
    }
}
