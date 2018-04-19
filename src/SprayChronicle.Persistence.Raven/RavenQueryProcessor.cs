using Raven.Client.Documents.Indexes;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class RavenQueryProcessor<TProcessor,TState> : IExecute, IProcess
        where TProcessor : RavenQueryProcessor<TProcessor,TState>
        where TState : class
    {
        protected RavenProcessedFactory<TState> Process()
        {
            return new RavenProcessedFactory<TState>();
        }
        
        protected RavenProcessedFactory<TState> Process(string identity)
        {
            return new RavenProcessedFactory<TState>(identity);
        }

        protected RavenExecutedFactory<TState> Execute()
        {
            return new RavenExecutedFactory<TState>();
        }
        
        protected RavenExecutorFactory<TState,TFilter> Execute<TFilter>()
            where TFilter : AbstractIndexCreationTask, new()
        {
            return new RavenExecutorFactory<TState,TFilter>();
        }
    }
}
