using System;
using Autofac;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.EventHandling.Projecting;

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
                .As<IBuildProjectionRepositories>()
                .SingleInstance();
            
            builder
                .Register<MongoProjectorFactory>(
                    c => new MongoProjectorFactory(
                        c.Resolve<ILogger<IStream>>(),
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
                        Console.WriteLine("Connected to MongoDB!");
                        return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DB") ?? "projections");
                    }
                )
                .SingleInstance();
        }
    }
}