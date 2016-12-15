using System;
using Autofac;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<MongoRepositoryFactory>(
                    c => new MongoRepositoryFactory(
                        c.Resolve<IMongoDatabase>()
                    )
                )
                .AsSelf()
                .As<IBuildStatefulRepositories>()
                .SingleInstance();
            
            builder
                .Register<MongoProjectorFactory>(
                    c => new MongoProjectorFactory(
                        c.Resolve<ILoggerFactory>(),
                        c.Resolve<MongoRepositoryFactory>()
                    )
                )
                .AsSelf()
                .As<IBuildProjectors>()
                .SingleInstance();

            builder
                .Register<IMongoDatabase>(
                    c => {
                        IMongoClient client = new MongoClient(string.Format(
                            "mongodb://{0}",
                            Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "127.0.0.1"
                        ));
                        c.Resolve<ILoggerFactory>().CreateLogger<IMongoDatabase>().LogInformation("Connected to MongoDB!");
                        return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DB") ?? "projections");
                    }
                )
                .SingleInstance();
        }
    }
}