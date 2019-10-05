using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace SprayChronicle.Mongo
{
    public class MongoInitializationService : BackgroundService
    {
        private readonly IMongoCollection<Envelope> _events;
        private readonly IMongoCollection<Snapshot> _snapshots;

        public MongoInitializationService(
            IMongoCollection<Envelope> events,
            IMongoCollection<Snapshot> snapshots
        )
        {
            _events = events;
            _snapshots = snapshots;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            await _events.Indexes.CreateManyAsync(
                new[] {
                    new CreateIndexModel<Envelope>(
                        Builders<Envelope>.IndexKeys
                            .Ascending(e => e.Epoch)
                    ),
                    new CreateIndexModel<Envelope>(
                        Builders<Envelope>.IndexKeys
                            .Ascending(e => e.InvariantType)
                            .Ascending(e => e.InvariantId)
                            .Ascending(e => e.CausationId)
                    ),
                    new CreateIndexModel<Envelope>(
                        Builders<Envelope>.IndexKeys
                            .Ascending(e => e.InvariantType)
                            .Ascending(e => e.InvariantId)
                            .Ascending(e => e.Sequence),
                        new CreateIndexOptions {Sparse = true, Unique = true,}
                    )
                },
                cancellation
            );
        }
    }
}
