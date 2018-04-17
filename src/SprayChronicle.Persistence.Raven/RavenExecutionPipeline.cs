using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenExecutionPipeline<TProcessor,TState> : ExecutionPipeline<IAsyncDocumentSession,TProcessor>
        where TProcessor : RavenQueryProcessor<TProcessor,TState>
        where TState : class
    {
        private readonly IDocumentStore _store;

        public RavenExecutionPipeline(IDocumentStore store, TProcessor processor) : base(processor)
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
