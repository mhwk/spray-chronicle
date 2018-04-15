using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenQueryExecuted<TState> : QueryExecuted<IAsyncDocumentSession>
        where TState : class
    {
        private Func<IRavenQueryable<TState>,Task<TState>> _single;

        private Func<IRavenQueryable<TState>,Task<List<TState>>> _multiple;

        public Task<RavenQueryExecuted<TState>> Query(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            _single = query;
            return Task.FromResult(this);
        }

        public Task<RavenQueryExecuted<TState>> Query(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            _multiple = query;
            return Task.FromResult(this);
        }

        public override Task<object> Do(IAsyncDocumentSession session)
        {
            if (null != _single && null != _multiple) {
                throw new Exception($"Both single and multiple query not supported");
            }

            if (null != _single) {
                return _single(session.Query<TState>()) as Task<object>;
            }

            return _multiple(session.Query<TState>()) as Task<object>;
        }
    }
    
    public sealed class RavenQueryExecuted<TState,TFilter> : QueryExecuted<IAsyncDocumentSession>
        where TState : class
        where TFilter : AbstractIndexCreationTask, new()
    {
        private Func<IRavenQueryable<TState>,Task<TState>> _single;

        private Func<IRavenQueryable<TState>,Task<List<TState>>> _multiple;

        public Task<RavenQueryExecuted<TState,TFilter>> Query(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            _single = query;
            return Task.FromResult(this);
        }

        public Task<RavenQueryExecuted<TState,TFilter>> Query(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            _multiple = query;
            return Task.FromResult(this);
        }

        public Task<RavenQueryExecuted<TResult,TFilter>> Query<TResult>(Func<IRavenQueryable<TResult>,Task<TResult>> query)
            where TResult : class
        {
            return new RavenQueryExecuted<TResult, TFilter>().Query(query);
        }

        public Task<RavenQueryExecuted<TResult,TFilter>> Query<TResult>(Func<IRavenQueryable<TResult>,Task<List<TResult>>> query)
            where TResult : class
        {
            return new RavenQueryExecuted<TResult, TFilter>().Query(query);
        }

        public override Task<object> Do(IAsyncDocumentSession session)
        {
            if (null != _single && null != _multiple) {
                throw new Exception($"Both single and multiple query not supported");
            }

            if (null != _single) {
                return _single(session.Query<TState,TFilter>()) as Task<object>;
            }

            return _multiple(session.Query<TState,TFilter>()) as Task<object>;
        }
    }
}
