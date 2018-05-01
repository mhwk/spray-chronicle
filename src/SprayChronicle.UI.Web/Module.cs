using Autofac;
using SprayChronicle.Server.Http;

namespace SprayChronicle.UI.Web
{
    public sealed class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new ContextAttributeProvider<HttpQueryAttribute>())
                .AsSelf()
                .As<IAttributeProvider<HttpQueryAttribute>>();
        }
    }
}