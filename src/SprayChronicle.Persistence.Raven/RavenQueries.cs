using Raven.Client.Documents.Indexes;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class RavenQueries<TProcessor,TState> : IExecute, IProcess
        where TProcessor : RavenQueries<TProcessor,TState>
        where TState : class
    {
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
