using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace SprayChronicle.Mongo
{
    public class MongoMigration : IBackgroundTask
    {
        private readonly ILogger<MongoMigration> _logger;
        private readonly IMongoDatabase _database;
        private readonly string _from;
        private readonly string _to;

        public MongoMigration(
            ILogger<MongoMigration> logger,
            IMongoDatabase database,
            string from,
            string to
        )
        {
            _logger = logger;
            _database = database;
            _from = from;
            _to = to;
        }
        
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (null == _from) {
                _logger.LogDebug("No from specified, skipping migration");
                return;
            }

            if (null == _to) {
                _logger.LogDebug("No to specified, skipping migration");
                return;
            }
            
            var from = _database.GetCollection<BsonDocument>(_from);
            var to = _database.GetCollection<BsonDocument>(_to);

            if (await from.AsQueryable().CountAsync(stoppingToken) == await to.AsQueryable().CountAsync(stoppingToken)) {
                _logger.LogInformation("From and to count equals, no migration needed");
            }

            foreach (var document in from
                .AsQueryable()
                .OrderBy(d => d["Epoch"])
                .Skip(await to.AsQueryable().CountAsync(stoppingToken))) {
                document["_id"] = null;
                await to.InsertOneAsync(
                    document,
                    new InsertOneOptions {BypassDocumentValidation = true},
                    stoppingToken
                );
                _logger.LogInformation($"Migrated document for {document["InvariantType"]}: {document["InvariantId"]}");
            }
        }
    }
}
