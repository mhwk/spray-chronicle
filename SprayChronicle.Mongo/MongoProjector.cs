using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace SprayChronicle.Mongo
{
    public sealed class MongoProjector<TProjector> : Projector<TProjector>
        where TProjector : IProject
    {
        private readonly IMongoDatabase _database;
        private readonly List<WriteModel<BsonDocument>> _operations;
        private Checkpoint? _checkpoint;

        public MongoProjector(
            ILogger<TProjector> logger,
            IStoreEvents events,
            IMongoDatabase database,
            TProjector process,
            int batchSize,
            TimeSpan timeout
        ) : base(
            logger,
            events,
            process,
            batchSize,
            timeout
        )
        {
            _database = database;
            _operations = new List<WriteModel<BsonDocument>>();
        }

        protected override async Task<Checkpoint> Checkpoint()
        {
            return _checkpoint ??= await _database
               .GetCollection<Checkpoint>(typeof(Checkpoint).Name)
               .AsQueryable()
               .Where(c => c.Id == typeof(TProjector).Name)
               .FirstOrDefaultAsync() ?? new Checkpoint {
                   Id = typeof(TProjector).Name,
                   Value = null,
               };
        }

        protected override async Task Commit(ProjectionResult[] results)
        {
            using var session = await _database.Client.StartSessionAsync();

            try {
                session.StartTransaction();

                var mutations = results
                    .Where(x => x.Projection.GetType().IsGenericType && x.Projection.GetType().Name == typeof(Projection.Mutate<>).Name)
                    .Select(x => new {
                        Type = (Type)x.Projection.GetType().GetProperty("Type").GetValue(x.Projection),
                        Identity = (string)x.Projection.GetType().GetProperty("Identity").GetValue(x.Projection),
                        Mutate = x.Projection.GetType().GetMethod("DoMutate"),
                        Envelope = x.Envelope,
                        Projection = x.Projection
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
                        (await Checkpoint()).Value = x.Envelope.MessageId;
                        states[x.Identity] = x.Mutate
                            .Invoke(
                                x.Projection,
                                new[] {
                                    states.ContainsKey(x.Identity)
                                        ? states[x.Identity]
                                        : null
                                }
                            );
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

                await _database
                    .GetCollection<Checkpoint>(typeof(Checkpoint).Name)
                    .BulkWriteAsync(
                        new[] {
                            new ReplaceOneModel<Checkpoint>(
                                new BsonDocument("_id", typeof(TProjector).Name),
                                await Checkpoint()
                            ) {IsUpsert = true},
                        },
                        new BulkWriteOptions {IsOrdered = false, BypassDocumentValidation = true}
                    );

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
