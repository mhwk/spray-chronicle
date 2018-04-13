using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenQueryScope<TState, TResult>
        : IQueryScope<TState, TResult, IDocumentSession>
    {
        private readonly List<Func<TState, Task<TResult>>> _mutations = new List<Func<TState, Task<TResult>>>();

        private readonly List<QueryMetadata<IDocumentSession>> _queries = new List<QueryMetadata<IDocumentSession>>();

        public string Identity { get; }

        public Func<TState,Task<TResult>>[] Mutations {
            get {
                var mutations = _mutations.ToArray();
                _mutations.Clear();
                return mutations;
            }
        }
    
        public QueryMetadata<IDocumentSession>[] Queries {
            get {
                var queries = _queries.ToArray();
                _queries.Clear();
                return queries;
            }
        }

        public RavenQueryScope(): this(null)
        {
        }
        
        public RavenQueryScope(string identity)
        {
            Identity = identity;
        }

        public Task Mutate(Func<TResult> mutate)
        {
            return Mutate(m => mutate());
        }

        public Task Mutate(Func<TState,TResult> mutate)
        {
            return Mutate(state => Task.FromResult(mutate(state)));
        }

        public Task Mutate(Func<TState,Task<TResult>> mutate)
        {
            _mutations.Add(mutate);

            return Task.CompletedTask;
        }

        public Task<QueryMetadata> Query<TFilter>(Func<IRavenQueryable<TResult>,Task<TResult>> query)
            where TFilter : AbstractIndexCreationTask, new()
        {
            return Task.FromResult(new QueryMetadata<IDocumentSession> {
                FirstOrDefault = session => query(session.Query<TResult,TFilter>()) as Task<object>
            } as QueryMetadata);
        }

        public Task<QueryMetadata> Query<TFilter>(Func<IRavenQueryable<TResult>,Task<List<TResult>>> query)
            where TFilter : AbstractIndexCreationTask, new()
        {
            return Task.FromResult(new QueryMetadata<IDocumentSession> {
                ToList = session => query(session.Query<TResult, TFilter>()) as Task<IEnumerable>
            } as QueryMetadata);
        }

    }
}
