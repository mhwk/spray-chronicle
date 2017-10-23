using Autofac;

namespace SprayChronicle.Persistence.Mongo
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterMongoPersistence(this ContainerBuilder builder)
        {
            builder.RegisterModule<MongoModule>();
            return builder;
        }
    }
}
