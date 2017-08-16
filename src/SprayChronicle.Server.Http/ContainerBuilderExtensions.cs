using Autofac;
using Autofac.Core;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.Persistence.Mongo;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public static class ContainerBuilderExtensions
    {
        public static void UseChronicleHttp(this ContainerBuilder builder)
        {
            builder.RegisterModule<SprayChronicleHttpModule>();
        }
    }
}