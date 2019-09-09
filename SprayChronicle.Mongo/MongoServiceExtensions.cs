using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace SprayChronicle.Mongo
{
    public static class MongoServiceExtensions
    {
        public static IEventSourcingBuilder AddMongo(this IServiceCollection services)
        {
            services.AddSingleton<IMongoClient>(s => {
                var options = s.GetRequiredService<IOptions<MongoOptions>>().Value;

                var mappers = s.GetServices<Action<IMapState>>().ToList();
                foreach (var mapper in mappers) {
                    mapper(new MongoStateMapper(s));
                }

                BsonSerializer.RegisterDiscriminatorConvention(
                    typeof(object),
                    new ScalarDiscriminatorConvention("_t")
                );
                BsonClassMap.RegisterClassMap<Envelope<object>>(map => {
                    map.AutoMap();
                    map.MapIdProperty(p => p.MessageId);
                });
                BsonClassMap.RegisterClassMap<Snapshot>(map => {
                    map.AutoMap();
                    map.MapIdProperty(p => p.SnapshotId);
                });

                return new MongoClient(options.ConnectionString);
            });
            services.AddSingleton<IMongoDatabase>(s => s
                .GetRequiredService<IMongoClient>()
                .GetDatabase(
                    s.GetRequiredService<IOptions<MongoOptions>>().Value.Database
                ));
            services.AddSingleton(s => new List<IMapState>());
            services.AddSingleton(s => new MongoStore(
                s.GetRequiredService<ILoggerFactory>().CreateLogger<MongoStore>(),
                s.GetRequiredService<IClientSessionHandle>(),
                s.GetRequiredService<IMongoCollection<Envelope<object>>>(),
                s.GetRequiredService<IMongoCollection<Snapshot>>()
            ));
            services.AddSingleton(s => s
                .GetRequiredService<IMongoDatabase>()
                .GetCollection<Envelope<object>>(
                    s.GetService<IOptions<MongoOptions>>().Value.EventCollection
                ));
            services.AddSingleton(s => s
                .GetRequiredService<IMongoDatabase>()
                .GetCollection<Snapshot>(
                    s.GetService<IOptions<MongoOptions>>().Value.SnapshotCollection
                )
            );
            services.AddSingleton<IStoreEvents>(s => s.GetRequiredService<MongoStore>());
            services.AddSingleton<IStoreSnapshots>(s => s.GetRequiredService<MongoStore>());
            services.AddTransient<IClientSessionHandle>(s => s.GetService<IMongoClient>().StartSession());
            services.AddTransient<IHostedService>(s => new MongoInitializationService(
                s.GetRequiredService<IMongoCollection<Envelope<object>>>(),
                s.GetRequiredService<IMongoCollection<Snapshot>>()
            ));

            return new MongoServiceBuilder(services);
        }
    }
}
