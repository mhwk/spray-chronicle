using System;
using Autofac;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
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
                        c.Resolve<IMongoDatabase>(),
                        c.Resolve<ILoggerFactory>()
                    )
                )
                .AsSelf()
                .As<IBuildStatefulRepositories>()
                .SingleInstance();
            
            builder
                .Register<MongoProjectorFactory>(
                    c => new MongoProjectorFactory(
                        c.Resolve<MongoRepositoryFactory>()
                    )
                )
                .AsSelf()
                .As<IBuildProjectors>()
                .SingleInstance();
            
            builder
                .Register<ILogger<IMongoDatabase>>(c => c.Resolve<ILoggerFactory>().CreateLogger<IMongoDatabase>())
                .SingleInstance();

            builder
                .Register<IMongoClient>(c => {
                    var client = new MongoClient(string.Format(
                        "mongodb://{0}",
                        Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "127.0.0.1"
                    ));
                    c.Resolve<ILogger<IMongoDatabase>>().LogInformation("Connected to MongoDB!");
                    return client;
                })
                .SingleInstance();
            
            builder
                .Register<DisposableDatabaseFactory>(c => new DisposableDatabaseFactory(c.Resolve<IMongoClient>(), c.Resolve<ILogger<IMongoClient>>()))
                .SingleInstance();

            builder
                .Register<IMongoDatabase>(c => c.Resolve<DisposableDatabaseFactory>().Build(DatabaseName()))
                .SingleInstance();
        }

        static string DatabaseName()
        {
            return Environment.GetEnvironmentVariable("MONGODB_DB") ?? GenerateRandomDatabaseName();
        }

        private static string GenerateRandomDatabaseName()
        {
            return $"{DateTime.Now.ToUniversalTime():yyyy_MM_dd_HH_mm_ss_ffffff}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        }
    }
}
