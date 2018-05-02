using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenExecutionPipeline<TProcessor,TState> : ExecutionPipeline<TProcessor>
        where TProcessor : RavenQueries<TProcessor,TState>
        where TState : class
    {
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
            if (!(executor is RavenExecuted raven)) {
                throw new Exception($"Executor is expected to be {typeof(RavenExecuted)}, {executor.GetType()} given");
            }
            
            using (var session = _store.OpenAsyncSession()) {
                var result = raven.Do(session);
                return await result;
            }
        }
    }
}
