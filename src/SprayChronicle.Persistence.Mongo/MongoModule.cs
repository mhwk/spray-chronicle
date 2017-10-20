using System;
using Autofac;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
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
                .Register<IMongoDatabase>(
                    c => {
                        var logger = c.Resolve<ILoggerFactory>().CreateLogger<IMongoDatabase>();
                        var client = new MongoClient(string.Format(
                            "mongodb://{0}",
                            Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "127.0.0.1"
                        ));
                        logger.LogInformation("Connected to MongoDB!");

                        var dbName = Environment.GetEnvironmentVariable("MONGODB_DB");
                        if (null == dbName) {
                            dbName = Guid.NewGuid().ToString();
                            logger.LogInformation("Using temporary database {0}", dbName);
                            System.AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                                client.DropDatabase(dbName);
                                logger.LogInformation("Cleaned up temporary database {0}", dbName);
                            };
                        }
                        return client.GetDatabase(dbName);
                    }
                )
                .SingleInstance();
        }
    }
}
