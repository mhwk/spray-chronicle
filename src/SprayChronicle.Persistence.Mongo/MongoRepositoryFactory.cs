using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoRepositoryFactory : IBuildStatefulRepositories
    {
        readonly IMongoDatabase _database;

        readonly ILoggerFactory _loggerFactory;

        public MongoRepositoryFactory(
            IMongoDatabase database,
            ILoggerFactory loggerFactory)
        {
            _database = database;
            _loggerFactory = loggerFactory;
        }

        public IStatefulRepository<T> Build<T>()
        {
            return Build<T>(typeof(T).Name);
        }

        public IStatefulRepository<T> Build<T>(string reference)
        {
            return new BufferedStateRepository<T>(
                _loggerFactory.CreateLogger<T>(),
                new MongoRepository<T>(_database.GetCollection<T>(reference))
            );
        }
    }
}
