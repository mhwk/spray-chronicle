using MongoDB.Driver;
using SprayChronicle.Projecting;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoRepositoryFactory : IBuildProjectionRepositories
    {
        readonly IMongoDatabase _database;

        public MongoRepositoryFactory(IMongoDatabase database)
        {
            _database = database;
        }

        public IStatefulRepository<T> Build<T>()
        {
            return Build<T>(typeof(T).Name);
        }

        public IStatefulRepository<T> Build<T>(string reference)
        {
            return new MongoRepository<T>(_database.GetCollection<T>(reference));
        }
    }
}
