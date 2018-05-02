using System;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenExecutedSingle<TState> : RavenExecuted
        where TState : class
    {
        private readonly Func<IRavenQueryable<TState>,Task<TState>> _query;

        public RavenExecutedSingle(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await _query(session.Query<TState>());
        }
    }
    
    public sealed class RavenExecutedSingle<TState,TFilter> : RavenExecuted
        where TState : class
        where TFilter : AbstractIndexCreationTask, new()
    {
        private readonly Func<IRavenQueryable<TState>,Task<TState>> _query;

        public RavenExecutedSingle(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            _query = query;
        }
        
        internal override async Task<object> Do(IAsyncDocumentSession session)
        {
            return await _query(session.Query<TState,TFilter>());
        }
    }
}
