using Autofac;
using Autofac.Core;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterChronicleHttp(this ContainerBuilder builder)
        {
            builder.RegisterModule<SprayChronicleHttpModule>();
            builder.RegisterMemoryPersistence();
            builder.RegisterCommandHandling();
            builder.RegisterEventHandling();
            builder.RegisterProjecting();
            builder.RegisterQueryHandling();
        }
    }
}