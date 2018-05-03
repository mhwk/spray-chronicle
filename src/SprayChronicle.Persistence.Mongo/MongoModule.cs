using System;
using Autofac;
using MongoDB.Driver;
using SprayChronicle.Server;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
//            builder
//                .Register<MongoRepositoryFactory>(
//                    c => new MongoRepositoryFactory(
//                        c.Resolve<IMongoDatabase>(),
//                        c.Resolve<ILoggerFactory>()
//                    )
//                )
//                .AsSelf()
//                .As<IBuildStatefulRepositories>()
//                .SingleInstance();
            
//            builder
//                .Register<MongoProjectorFactory>(
//                    c => new MongoProjectorFactory(
//                        c.Resolve<MongoRepositoryFactory>()
//                    )
//                )
//                .AsSelf()
//                .As<IBuildProjectors>()
//                .SingleInstance();
            
            builder
                .Register(c => c.Resolve<ILoggerFactory>().CreateLogger<IMongoDatabase>())
                .SingleInstance();

            builder
                .Register<IMongoClient>(c => {
                    var client = new MongoClient(string.Format(
                        "mongodb://{0}",
                        ChronicleServer.Env("MONGODB_HOST", "127.0.0.1")
                    ));
                    c.Resolve<Microsoft.Extensions.Logging.ILogger<IMongoDatabase>>().LogInformation("Connected to MongoDB!");
                    return client;
                })
                .SingleInstance();
        }
    }
}
