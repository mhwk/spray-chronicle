using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoRepositoryFactory : IBuildStatefulRepositories
    {
        private readonly IMongoDatabase _database;

        private readonly ILoggerFactory _loggerFactory;

        public MongoRepositoryFactory(
            IMongoDatabase database,
            ILoggerFactory loggerFactory)
        {
            _database = database;
            _loggerFactory = loggerFactory;
        }

        public IStatefulRepository<T> Build<T>()
            where T : class
        {
            return new BufferedStateRepository<T>(
                _loggerFactory.CreateLogger<T>(),
                new MongoRepository<T>(_database)
            );
        }

        public IStatefulRepository<T> Build<T>(string reference)
            where T : class
        {
            return new BufferedStateRepository<T>(
                _loggerFactory.CreateLogger<T>(),
                new MongoRepository<T>(_database, reference)
            );
        }
    }
}
