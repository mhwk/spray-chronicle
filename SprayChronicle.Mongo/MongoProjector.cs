using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace SprayChronicle.Mongo
{
    public sealed class MongoProjector<TProjector> : Projector<TProjector>
        where TProjector : IProject
    {
        private readonly IMongoDatabase _database;
        private readonly List<WriteModel<BsonDocument>> _operations;

        public MongoProjector(
            ILogger<TProjector> logger,
            IStoreEvents events,
            IStoreSnapshots snapshots,
            IMongoDatabase database,
            TProjector process,
            int batchSize,
            TimeSpan timeout
        ) : base(
            logger,
            events,
            snapshots,
            process,
            batchSize,
            timeout
        )
        {
            _database = database;
            _operations = new List<WriteModel<BsonDocument>>();
        }

        protected override async Task Commit(Projection[] projections)
        {
            using var session = await _database.Client.StartSessionAsync();

            try {
                session.StartTransaction();

                var mutations = projections
                    .Where(x => x.GetType().IsGenericType && x.GetType().Name == typeof(Projection.Mutate<>).Name)
                    .Select(x => new {
                        Type = (Type)x.GetType().GetProperty("Type").GetValue(x),
                        Identity = (string)x.GetType().GetProperty("Identity").GetValue(x),
                        Mutate = x.GetType().GetMethod("DoMutate"),
                        Projection = x
                    })
                    .GroupBy(x => x.Type)
                    .ToArray();

                foreach (var mutation in mutations) {
                    var collection = _database.GetCollection<BsonDocument>(mutation.Key.Name);
                    var ids = mutation.Select(x => x.Identity).ToArray();
                    var states = (await (await collection.FindAsync(
                        Builders<BsonDocument>.Filter.In(x => (string)x["_id"], ids)
                    )).ToListAsync()).ToDictionary(
                        x => (string)x["_id"],
                        x => BsonSerializer.Deserialize(x, mutation.Key));

                    foreach (var x in mutation) {
                        states[x.Identity] = x.Mutate
                            .Invoke(
                                x.Projection,
                                new[] {
                                    states.ContainsKey(x.Identity)
                                        ? states[x.Identity]
                                        : null
                                })
                            .ToBsonDocument();

                    }

                    foreach (var (identity, state) in states) {
                        if (null != state) {
                            _operations.Add(new ReplaceOneModel<BsonDocument>(
                                new BsonDocument("_id", identity),
                                state.ToBsonDocument()
                            ) {IsUpsert = true});
                        } else {
                            _operations.Add(new DeleteOneModel<BsonDocument>(
                                new BsonDocument("_id", identity)
                            ));
                        }
                    }

                    await collection.BulkWriteAsync(
                        _operations,
                        new BulkWriteOptions {IsOrdered = false, BypassDocumentValidation = true}
                    );
                }

                session.CommitTransaction();
            } catch (Exception) {
                session.AbortTransaction();
                throw;
            } finally {
                _operations.Clear();
            }
        }
    }
}
