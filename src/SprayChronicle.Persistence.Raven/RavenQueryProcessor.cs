using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class RavenQueryProcessor<TProcessor,TState> : IExecute, IProcess
        where TProcessor : RavenQueryProcessor<TProcessor,TState>
        where TState : class
    {
        protected EventProcessed<TState> Process()
        {
            return new EventProcessed<TState>();
        }
        
        protected EventProcessed<TState> Process(string identity)
        {
            return new EventProcessed<TState>(identity);
        }

        protected RavenQueryExecuted<TState> Execute()
        {
            return new RavenQueryExecuted<TState>();
        }
        
        protected RavenQueryExecuted<TState,TFilter> Execute<TFilter>()
            where TFilter : AbstractIndexCreationTask, new()
        {
            return new RavenQueryExecuted<TState,TFilter>();
        }
    }
}
