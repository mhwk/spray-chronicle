using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace SprayChronicle.Persistence.Mongo
{
    public class DisposableDatabaseFactory : IDisposable
    {
        readonly IMongoClient _client;

        readonly ILogger<IMongoClient> _logger;

        readonly List<string> _databases = new List<string>();

        public DisposableDatabaseFactory(IMongoClient client, ILogger<IMongoClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public IMongoDatabase Build()
        {
            return Build(Guid.NewGuid().ToString());
        }

        public IMongoDatabase Build(string databaseName)
        {
            _databases.Add(databaseName);
            return _client.GetDatabase(databaseName);
        }

        public void Dispose()
        {
            _databases.ForEach(database => {
                _logger.LogDebug("Disposing database {0}", database);
                _client.DropDatabase(database);
            });
            _logger.LogInformation("Disposed all databases");
        }
    }
}
