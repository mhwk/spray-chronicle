using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Mongo
{
    public class MongoRepository<T> : StatefulRepository<T> where T : class
    {
        private readonly IMongoDatabase _database;

        private readonly string _name;

        public MongoRepository(IMongoDatabase database) : this(database, typeof(T).Name)
        {}

        public MongoRepository(IMongoDatabase database, string name)
        {
            _database = database;
            _name = name;
        }

        public override string Identity(T obj)
        {
            return (string) obj.ToBsonDocument().GetElement("_id").Value;
        }

        public override T Load(string identity)
        {
            return _database
                .GetCollection<T>(_name)
                .Find(Builders<T>.Filter.Eq("_id", identity))
                .FirstOrDefault();
        }

        public override T Load(Func<IQueryable<T>,T> callback)
        {
            return callback(_database.GetCollection<T>(_name).AsQueryable());
        }

        public override IEnumerable<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback)
        {
            return callback(_database.GetCollection<T>(_name).AsQueryable());
        }

        public override PagedResult<T> Load(Func<IQueryable<T>,IEnumerable<T>> callback, int page, int perPage)
        {
            var results = callback(_database.GetCollection<T>(_name).AsQueryable()); 
            return new PagedResult<T>(
                results.Skip((page - 1) * perPage).Take(perPage),
                page,
                perPage,
                results.Count()
            );
        }

        public override void Save(T obj)
        {
            _database.GetCollection<T>(_name).FindOneAndReplace(
                Builders<T>.Filter.Eq("_id", Identity(obj)),
                obj,
                new FindOneAndReplaceOptions<T> {
                    IsUpsert = true,
                    BypassDocumentValidation = true
                }
            );
        }

        public override void Save(T[] objs)
        {
            var models = new WriteModel<T>[objs.Length];
            
            for (var i = 0; i < objs.Length; i++) {
                models[i] = new ReplaceOneModel<T>(new BsonDocument("_id", Identity(objs[i])), objs[i]) {
                    IsUpsert = true
                };
            }

            _database.GetCollection<T>(_name).BulkWrite(models, new BulkWriteOptions() { IsOrdered = false, BypassDocumentValidation = true });
        }

        public override void Remove(string identity)
        {
            Remove(new string[1] {identity});
        }

        public override void Remove(string[] identities)
        {
            _database.GetCollection<T>(_name).DeleteMany(Builders<T>.Filter.In("_id", new BsonArray(identities)));
        }

        public override void Remove(T obj)
        {
            Remove(Identity(obj));
        }

        public override void Remove(T[] objs)
        {
            Remove(objs.Select(Identity).ToArray());
        }

        public override void Clear()
        {
            _database.DropCollection(_name);
        }
    }
}
