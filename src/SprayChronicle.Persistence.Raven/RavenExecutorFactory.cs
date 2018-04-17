using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenExecutedFactory<TState>
        where TState : class
    {
        public Task<RavenExecutorSingle<TState>> Query(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            return Task.FromResult(new RavenExecutorSingle<TState>(query));
        }

        public Task<RavenExecutorMultiple<TState>> Query(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            return Task.FromResult(new RavenExecutorMultiple<TState>(query));
        }
    }
    
    public sealed class RavenExecutorFactory<TState,TFilter>
        where TState : class
        where TFilter : AbstractIndexCreationTask, new()
    {
        public Task<RavenExecutorSingle<TState,TFilter>> Query(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            return Task.FromResult(new RavenExecutorSingle<TState,TFilter>(query));
        }

        public Task<RavenExecutorMultiple<TResult,TFilter>> Query<TResult>(Func<IRavenQueryable<TResult>,Task<List<TResult>>> query)
            where TResult : class
        {
            return Task.FromResult(new RavenExecutorMultiple<TResult,TFilter>(query));
        }
    }
}
