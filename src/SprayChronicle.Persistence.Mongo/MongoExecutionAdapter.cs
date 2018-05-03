using System;
using System.Threading.Tasks;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoExecutionAdapter<TProcessor,TState> : IQueryExecutionAdapter
        where TProcessor : MongoQueries<TProcessor,TState>
        where TState : class
    {
        private readonly ILogger<TProcessor> _logger;
        
        private readonly IDocumentStore _store;

        public RavenExecutionAdapter(
            ILogger<TProcessor> logger,
            IDocumentStore store)
        {
            _logger = logger;
            _store = store;
        }

        public async Task<object> Apply(Executed executed)
        {
            if (!(executed is MongoExecuted mongo)) {
                throw new Exception($"Executed is expected to be {typeof(RavenExecuted)}, {executed.GetType()} given");
            }
            
            using (var session = _store.OpenAsyncSession()) {
                var result = mongo.Do(session);
                return await result;
            }
        }
    }
}
