using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace SprayChronicle.Mongo
{
    public sealed class MongoStore : IStoreEvents, IStoreSnapshots, IDisposable
    {
        private readonly ILogger<MongoStore> _logger;
        private readonly IClientSessionHandle _session;
        private readonly IMongoCollection<Envelope> _events;
        private readonly IMongoCollection<Snapshot> _snapshots;
        private readonly IDiscriminatorConvention _discriminatorConvention;

        public MongoStore(
            ILogger<MongoStore> logger,
            IClientSessionHandle session,
            IMongoCollection<Envelope> events,
            IMongoCollection<Snapshot> snapshots
        )
        {
            _logger = logger;
            _session = session;
            _events = events;
            _snapshots = snapshots;
            _discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
        }

        public async IAsyncEnumerable<Envelope> Load(
            Checkpoint? checkpoint,
            CancellationToken cancellation
        )
        {
            var from = CheckpointToFilter(checkpoint);

            using var cursor = await _events
                .AsQueryable()
                .OrderBy(x => x.MessageId)
                .Where(x => from.Inject())
                .ToCursorAsync(cancellation);

            while (await cursor.MoveNextAsync(cancellation)) {
                foreach (var envelope in cursor.Current) {
                    yield return envelope;
                }
            }
        }

        public async IAsyncEnumerable<Envelope> Watch(
            Checkpoint? checkpoint,
            CancellationToken cancellation
        )
        {
            var envelopes = Load(checkpoint, cancellation);
            checkpoint ??= new Checkpoint();
            await foreach (var envelope in envelopes) {
                checkpoint.Value = envelope.MessageId;
                yield return envelope;
            }
            
            var options = new ChangeStreamOptions {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
            };
            var from = CheckpointToFilter(checkpoint);

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Envelope>>()
                .Match(x => x.OperationType == ChangeStreamOperationType.Insert)
                .Match(x => from.Inject());

            using var cursor = await _events.WatchAsync(pipeline, options, cancellation);

            while (await cursor.MoveNextAsync(cancellation)) {
                foreach (var change in cursor.Current) {
                    yield return change.FullDocument;
                }
            }
        }

        public async IAsyncEnumerable<Envelope> Load<TInvariant>(
            string invariantId,
            string causationId,
            long fromPosition
        )
        {
            if (!_session.IsInTransaction) {
                _session.StartTransaction();
            }

            var envelopes = await _events.AsQueryable()
                .Where(x => x.InvariantType == typeof(TInvariant).Name)
                .Where(x => x.InvariantId == invariantId)
                .Where(x => x.Sequence > fromPosition)
                .OrderBy(x => x.Sequence)
                .ToCursorAsync();

            while (envelopes.MoveNext()) {
                foreach (var envelope in envelopes.Current) {
                    if (null != causationId && envelope.CausationId == causationId) throw new IdempotencyException(causationId);
                    yield return envelope;
                }
            }
        }

        public async Task Append<TInvariant>(IEnumerable<Envelope> envelopes)
        {
            if (!_session.IsInTransaction) {
                _session.StartTransaction();
            }

            try {
                await _events.InsertManyAsync(
                    envelopes.Select(e => e.IdentifiedBy(ObjectId.GenerateNewId().ToString())),
                    new InsertManyOptions {
                        IsOrdered = true,
                    }
                );
                await _session.CommitTransactionAsync();
            } catch (Exception error) {
                await _session.AbortTransactionAsync();
                throw;
            }
        }
        
        public async Task<Snapshot> Load<TSnap>(
            string invariantId,
            string causationId
        )
        {
            if (null != causationId) {
                if (await _events
                    .AsQueryable()
                    .Where(e => e.InvariantType == typeof(TSnap).Name)
                    .Where(e => e.InvariantId == invariantId)
                    .AnyAsync(e => e.CausationId == causationId)) {
                    throw new IdempotencyException(causationId);
                }
            }
            
            var snapshot = await _snapshots.AsQueryable()
                .Where(d => d.Snap is TSnap)
                .Where(d => d.Identity == invariantId)
                .FirstOrDefaultAsync();

            return snapshot ?? BsonSerializer.Deserialize<Snapshot>(new BsonDocument {
                {"Sequence", -1},
                {"Identity", invariantId},
                {"Snap", new BsonDocument {{"_t", _discriminatorConvention.GetDiscriminator(typeof(object), typeof(TSnap))}}}
            });
        }

        public async Task Save<TInvariant>(Snapshot snapshot)
        {
            await _snapshots.ReplaceOneAsync(
                s => s.SnapshotId == snapshot.SnapshotId,
                snapshot,
                new UpdateOptions {IsUpsert = true}
            );
        }

        private static FilterDefinition<BsonDocument> CheckpointToFilter(Checkpoint? checkpoint)
        {
            if (checkpoint?.Value == null)
            {
                return Builders<BsonDocument>.Filter.Empty;
            }

            return Builders<BsonDocument>.Filter.Gt(
                x => x["_id"],
                checkpoint.Value
            );
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
