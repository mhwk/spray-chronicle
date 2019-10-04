using System;
using System.Collections.Generic;
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
        private readonly IMongoCollection<Envelope<object>> _events;
        private readonly IMongoCollection<Snapshot> _snapshots;
        private readonly IDiscriminatorConvention _discriminatorConvention;

        public MongoStore(
            ILogger<MongoStore> logger,
            IClientSessionHandle session,
            IMongoCollection<Envelope<object>> events,
            IMongoCollection<Snapshot> snapshots
        )
        {
            _logger = logger;
            _session = session;
            _events = events;
            _snapshots = snapshots;
            _discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
        }

        public async IAsyncEnumerable<Envelope<object>> Load(
            DateTime? since,
            CancellationToken cancellation
        )
        {
            using var cursor = await _events.AsQueryable()
                .OrderBy(x => x.Epoch)
                .Where(x => null == since || x.Epoch > since)
                .ToCursorAsync(cancellation);

            while (await cursor.MoveNextAsync(cancellation)) {
                foreach (var evt in cursor.Current) {
                    yield return evt;
                }
            }
        }

        public async IAsyncEnumerable<Envelope<object>> Watch(
            DateTime? since,
            CancellationToken cancellation
        )
        {
            var processed = new HashSet<string>();
            await foreach (var envelope in Load(since, cancellation)) {
                if (since < envelope.Epoch) {
                    since = envelope.Epoch;
                    processed.Clear();
                }

                processed.Add(envelope.MessageId);
                
                yield return envelope;
            }
            
            var options = new ChangeStreamOptions {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
                StartAtOperationTime = null == since ? null : new BsonTimestamp((int)((DateTimeOffset)since).ToUnixTimeSeconds(), 1),
            };

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Envelope<object>>>()
                .Match(x => x.OperationType == ChangeStreamOperationType.Insert);

            using var cursor = await _events.WatchAsync(pipeline, options, cancellation);

            while (await cursor.MoveNextAsync(cancellation)) {
                foreach (var change in cursor.Current) {
                    if (processed.Contains(change.FullDocument.MessageId)) {
                        _logger.LogDebug(
                            $"Deduplicated {change.FullDocument.Message.GetType()} with id {change.FullDocument.MessageId}");
                        continue;
                    }

                    yield return change.FullDocument;
                }
            }
        }

        public async IAsyncEnumerable<Envelope<object>> Load<TInvariant>(
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

        public async Task Append<TInvariant>(IEnumerable<Envelope<object>> envelopes)
        {
            if (!_session.IsInTransaction) {
                _session.StartTransaction();
            }

            try {
                await _events.InsertManyAsync(envelopes, new InsertManyOptions {
                    IsOrdered = true,
                });
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

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
