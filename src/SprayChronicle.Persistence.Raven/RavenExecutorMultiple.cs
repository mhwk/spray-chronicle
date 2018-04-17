using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenExecutorMultiple<TState> : RavenExecutor
        where TState : class
    {
        private readonly Func<IRavenQueryable<TState>,Task<List<TState>>> _query;

        public RavenExecutorMultiple(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await _query(session.Query<TState>());
        }
    }
    
    public class RavenExecutorMultiple<TState,TFilter> : RavenExecutor
        where TState : class
        where TFilter : AbstractIndexCreationTask, new()
    {
        private readonly Func<IRavenQueryable<TState>,Task<List<TState>>> _query;

        public RavenExecutorMultiple(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await _query(session.Query<TState,TFilter>());
        }
    }
}
