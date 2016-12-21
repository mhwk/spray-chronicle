using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Mongo
{
    public class MongoRepository<T> : IStatefulRepository<T>
    {
        IMongoCollection<T> _collection;

        public MongoRepository(IMongoCollection<T> collection)
        {
            _collection = collection;
        }

        public string Identity(T obj)
        {
            return (string) obj.ToBsonDocument().GetElement("_id").Value;
        }

        public T Load(string identity)
        {
            return _collection
                .Find(Builders<T>.Filter.Eq("_id", identity))
                .FirstOrDefault();
        }

        public void Save(T obj)
        {
            _collection.FindOneAndReplace(
                Builders<T>.Filter.Eq("_id", Identity(obj)),
                obj,
                new FindOneAndReplaceOptions<T> {
                    IsUpsert = true,
                    BypassDocumentValidation = true
                }
            );
        }

        public void Save(T[] objs)
        {
            var models = new WriteModel<T>[objs.Length];
            
            for (var i = 0; i < objs.Length; i++) {
                models[i] = new ReplaceOneModel<T>(new BsonDocument("_id", Identity(objs[i])), objs[i]) {
                    IsUpsert = true
                };
            }

            _collection.BulkWrite(models, new BulkWriteOptions() { IsOrdered = false, BypassDocumentValidation = true });
        }

        public void Remove(string identity)
        {
            Remove(new string[1] {identity});
        }

        public void Remove(string[] identities)
        {
            _collection.DeleteMany(Builders<T>.Filter.In("_id", new BsonArray(identities)));
        }

        public void Remove(T obj)
        {
            Remove(Identity(obj));
        }

        public void Remove(T[] objs)
        {
            Remove(objs.Select(obj => Identity(obj)).ToArray());
        }

        public void Clear()
        {
            _collection.DeleteMany(new BsonDocument());
        }

        public IQueryable<T> Query()
        {
            return _collection.AsQueryable<T>();
        }
    }
}
