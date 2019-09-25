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
            var time = DateTime.Now;

            session.StartTransaction();

            try {
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
                    var states = await (await collection.FindAsync(
                        Builders<BsonDocument>.Filter.In(x => (string)x["_id"], ids)
                    )).ToListAsync();

                    if (states.Any(x => !x.Contains("_id"))) {
                        throw new Exception($"No MongoID configured for state {mutation.Key}");
                    }

                    foreach (var x in mutation) {
                        var current = states.FirstOrDefault(s => (string)s["_id"] == x.Identity);
                        var next = x.Mutate.Invoke(x.Projection,
                                new[] {null == current ? null : BsonSerializer.Deserialize(current, x.Type)})
                            .ToBsonDocument();

                        if (null != next) {
                            if (null == current) {
                                states.Add(next);
                            }

                            _operations.Add(new ReplaceOneModel<BsonDocument>(
                                new BsonDocument("_id", x.Identity),
                                next
                            ) {IsUpsert = true});
                        } else if (null != current) {
                            states.Remove(current);

                            _operations.Add(new DeleteOneModel<BsonDocument>(
                                new BsonDocument("_id", x.Identity)
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
