using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenExecutionAdapter<TProcessor,TState> : IQueryExecutionAdapter
        where TProcessor : RavenQueries<TProcessor,TState>
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
            if (!(executed is RavenExecuted raven)) {
                throw new Exception($"Executed is expected to be {typeof(RavenExecuted)}, {executed.GetType()} given");
            }
            
            using (var session = _store.OpenAsyncSession()) {
                var result = raven.Do(session);
                return await result;
            }
        }
    }
}
