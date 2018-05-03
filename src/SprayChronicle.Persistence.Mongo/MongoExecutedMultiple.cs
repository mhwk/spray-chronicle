using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.Persistence.Mongo
{
    public class MongoExecutedMultiple<TState> : MongoExecuted
        where TState : class
    {
        private readonly Func<IRavenQueryable<TState>,Task<List<TState>>> _query;

        public RavenExecutedMultiple(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await _query(session.Query<TState>());
        }
    }
    
    public class MongoExecutedMultiple<TState,TFilter> : RavenExecuted
        where TState : class
        where TFilter : AbstractIndexCreationTask, new()
    {
        private readonly Func<IRavenQueryable<TState>,Task<List<TState>>> _query;

        public RavenExecutedMultiple(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await _query(session.Query<TState,TFilter>());
        }
    }
}
