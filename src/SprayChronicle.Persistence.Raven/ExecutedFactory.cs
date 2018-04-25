using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class ExecutedFactory<TState>
        where TState : class
    {
        public Task<ExecutedSingle<TState>> Query(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            return Task.FromResult(new ExecutedSingle<TState>(query));
        }

        public Task<ExecutedMultiple<TState>> Query(Func<IRavenQueryable<TState>,Task<List<TState>>> query)
        {
            return Task.FromResult(new ExecutedMultiple<TState>(query));
        }

        public Task<ExecutedFind<TState>> Find(string identity)
        {
            return Task.FromResult(new ExecutedFind<TState>(identity));
        }
    }
    
    public sealed class RavenExecutorFactory<TState,TFilter>
        where TState : class
        where TFilter : AbstractIndexCreationTask, new()
    {
        public Task<ExecutedSingle<TState,TFilter>> Query(Func<IRavenQueryable<TState>,Task<TState>> query)
        {
            return Task.FromResult(new ExecutedSingle<TState,TFilter>(query));
        }

        public Task<ExecutedMultiple<TResult,TFilter>> Query<TResult>(Func<IRavenQueryable<TResult>,Task<List<TResult>>> query)
            where TResult : class
        {
            return Task.FromResult(new ExecutedMultiple<TResult,TFilter>(query));
        }
    }
}
