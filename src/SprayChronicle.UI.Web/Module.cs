using Autofac;
using Microsoft.AspNetCore.Hosting;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;
using SprayChronicle.Server.Http;
using SprayChronicle.UI.Web.Application.Services;
using SprayChronicle.UI.Web.Infrastructure;

namespace SprayChronicle.UI.Web
{
    public sealed class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new ContextAttributeProvider<Module,HttpQueryAttribute>())
                .AsSelf()
                .As<IAttributeProvider<HttpQueryAttribute>>()
                .SingleInstance();
            
            
            builder
                .Register(c => new SpaExecutionPipeline<QuerySpa>(
                    c.Resolve<ILoggerFactory>().Create<QuerySpa>(),
                    new QuerySpa(),
                    c.Resolve<IHostingEnvironment>()))
                .AsSelf()
                .As<IPipeline>()
                .As<IMailStrategyRouterSubscriber<IExecute>>()
                .SingleInstance();

        }
    }
}