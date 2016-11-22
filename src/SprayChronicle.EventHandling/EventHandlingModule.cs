using System;
using System.Linq;
using Autofac;
using MongoDB.Driver;
using SprayChronicle.EventHandling.MongoDB;

namespace SprayChronicle.EventHandling
{
    public sealed class EventHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<IManageStreamHandlers>(c => new StreamHandlerManager())
                .OnActivating(e => RegisterStreamHandlers(e.Context, e.Instance as IManageStreamHandlers))
                .SingleInstance();

            LoadMongo(builder);
        }

        void RegisterStreamHandlers(IComponentContext context, IManageStreamHandlers manager)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleStream>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleStream)
                .ToList()
                .ForEach(h => manager.Add(h));
        }

        void LoadMongo(ContainerBuilder builder)
        {
            builder
                .Register<MongoRepositoryFactory>(
                    c => new MongoRepositoryFactory(
                        c.Resolve<IMongoDatabase>()
                    )
                )
                .SingleInstance();
            
            builder
                .Register<MongoProjectorFactory>(
                    c => new MongoProjectorFactory(
                        c.Resolve<MongoRepositoryFactory>()
                    )
                )
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