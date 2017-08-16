using Autofac;
using Autofac.Core;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.Persistence.Mongo;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server
{
    public static class ContainerBuilderExtensions
    {
        public static void UseModule<TModule>(this ContainerBuilder builder) where TModule : IModule, new()
        {
            builder.RegisterModule<TModule>();
        }

        public static void UseModule(this ContainerBuilder builder, IModule module)
        {
            builder.RegisterModule(module);
        }

        public static void UseChronicle(this ContainerBuilder builder)
        {
            builder.UseCommandHandling();
            builder.UseEventHandling();
            builder.UseQueryHandling();
            builder.UseMemoryPersistence();
        }

        public static void UseCommandHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<CommandHandlingModule>();
        }

        public static void UseEventHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<AsyncEventHandlingModule>();
        }

        public static void UseQueryHandling(this ContainerBuilder builder)
        {
            builder.RegisterModule<ProjectingModule>();
            builder.RegisterModule<QueryHandlingModule>();
        }

        public static void UseMemoryPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<MemoryModule>();
        }

        public static void UseMongoPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<MongoModule>();
        }

        public static void UseOuroPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<OuroModule>();
        }
    }
}