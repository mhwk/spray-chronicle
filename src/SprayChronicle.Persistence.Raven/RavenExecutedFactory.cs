using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenExecutedFactory<TState>
        where TState : class
    {
        public Task<RavenExecutedSingle<TState>> Query(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            return Task.FromResult(new RavenExecutedSingle<TState>(query));
        }

        public Task<RavenExecutedMultiple<TState>> Query(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            return Task.FromResult(new RavenExecutedMultiple<TState>(query));
        }

        public Task<RavenExecutedFind<TState>> Find(string identity)
        {
            return Task.FromResult(new RavenExecutedFind<TState>(identity));
        }
    }
    
    public sealed class RavenExecutorFactory<TState,TFilter>
        where TState : class
        where TFilter : AbstractIndexCreationTask, new()
    {
        public Task<RavenExecutedSingle<TState,TFilter>> Query(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            return Task.FromResult(new RavenExecutedSingle<TState,TFilter>(query));
        }

        public Task<RavenExecutedMultiple<TResult,TFilter>> Query<TResult>(Func<IRavenQueryable<TResult>,Task<List<TResult>>> query)
            where TResult : class
        {
            return Task.FromResult(new RavenExecutedMultiple<TResult,TFilter>(query));
        }
    }
}
