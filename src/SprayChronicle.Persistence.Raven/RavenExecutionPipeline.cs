using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenExecutionPipeline<TProcessor,TState> : ExecutionPipeline<TProcessor>
        where TProcessor : RavenQueryProcessor<TProcessor,TState>
        where TState : class
    {
        private readonly ILogger<TProcessor> _logger;
        
        private readonly IDocumentStore _store;

        public RavenExecutionPipeline(
            ILogger<TProcessor> logger,
            IDocumentStore store,
            TProcessor processor) : base(logger, processor)
        {
            _store = store;
        }

        protected override async Task<object> Apply(Executor executor)
        {
            if (!(executor is RavenExecutor raven)) {
                throw new Exception($"Executor is expected to be {typeof(RavenExecutor)}, {executor.GetType()} given");
            }
            
            using (var session = _store.OpenAsyncSession()) {
                return await raven.Do(session);
            }
        }
    }
}
