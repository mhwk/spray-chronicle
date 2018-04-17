using Raven.Client.Documents.Indexes;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class RavenQueryProcessor<TProcessor,TState> : IExecute, IProcess
        where TProcessor : RavenQueryProcessor<TProcessor,TState>
        where TState : class
    {
        protected RavenProcessorFactory<TState> Process()
        {
            return new RavenProcessorFactory<TState>();
        }
        
        protected RavenProcessorFactory<TState> Process(string identity)
        {
            return new RavenProcessorFactory<TState>(identity);
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
