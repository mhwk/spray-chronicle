using System;
using MongoDB.Bson.Serialization;

namespace SprayChronicle.Mongo
{
    public class MongoStateMapper : IMapState
    {
        private readonly IServiceProvider _serviceProvider;

        public MongoStateMapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public IMapState Map<T>()
        {
            BsonClassMap.RegisterClassMap<T>(x => {
                x.AutoMap();
                x.RequirePropertySetters();
            });
            return this;
        }

        public IMapState Map<T>(string name)
        {
            BsonClassMap.RegisterClassMap<T>(x => {
                x.AutoMap();
                x.SetDiscriminator(name);
                x.RequirePropertySetters();
            });
            return this;
        }

        public IMapState Map<T>(string name, Func<IServiceProvider, object> creator)
        {
            BsonClassMap.RegisterClassMap<T>(x => {
                x.AutoMap();
                x.SetDiscriminator(name);
                x.SetCreator(() => creator(_serviceProvider));
                x.RequirePropertySetters();
            });
            return this;
        }

        public IMapState Map<T>(Func<IServiceProvider, object> creator)
        {
            BsonClassMap.RegisterClassMap<T>(x => {
                x.AutoMap();
                x.SetCreator(() => creator(_serviceProvider));
                x.RequirePropertySetters();
            });
            return this;
        }

    }
}
