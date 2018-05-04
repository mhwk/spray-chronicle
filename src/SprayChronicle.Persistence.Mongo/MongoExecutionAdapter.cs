using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoExecutionAdapter<TProcessor,TState> : IQueryExecutionAdapter
        where TProcessor : MongoQueries<TProcessor,TState>
        where TState : class
    {
        private readonly ILogger<TProcessor> _logger;
        
        private readonly IMongoDatabase _database;

        public MongoExecutionAdapter(
            ILogger<TProcessor> logger,
            IMongoDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        public async Task<object> Apply(Executed executed)
        {
            if (!(executed is MongoExecuted mongo)) {
                throw new Exception($"Executed is expected to be {typeof(MongoExecuted)}, {executed.GetType()} given");
            }
            
            var result = mongo.Do(_database);
            return await result;
        }
    }
}
