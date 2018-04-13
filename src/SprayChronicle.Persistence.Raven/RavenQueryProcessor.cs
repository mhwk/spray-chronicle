using System.Collections.Generic;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class RavenQueryProcessor<TState> : IQueryProcessor
        where TState : class
    {
        private readonly List<IQueryScope> _scopes = new List<IQueryScope>();

        public RavenQueryScope<TState,TState> For()
        {
            var scope = new RavenQueryScope<TState,TState>();
            _scopes.Add(scope);
            return scope;
        }
        
        public RavenQueryScope<TState,TState> For(string identity)
        {
            var scope = new RavenQueryScope<TState,TState>(identity);
            _scopes.Add(scope);
            return scope;
        }
        
        public RavenQueryScope<TState,TResult> For<TResult>() where TResult : class
        {
            var scope = new RavenQueryScope<TState,TResult>();
            _scopes.Add(scope);
            return scope;
        }

        public IQueryScope[] Dequeue {
            get {
                var scopes = _scopes.ToArray();
                _scopes.Clear();
                return scopes;
            }
        }
    }
}
