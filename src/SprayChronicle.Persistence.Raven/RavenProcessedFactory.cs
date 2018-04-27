using System;
using System.Threading.Tasks;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenProcessedFactory<TState>
        where TState : class
    {
        private readonly string _identity;

        public RavenProcessedFactory()
        {
        }

        public RavenProcessedFactory(string identity)
        {
            _identity = identity;
        }

        public Task<ProcessedCreate<TState>> Mutate(Func<TState> mutator)
        {
            return Task.FromResult(new ProcessedCreate<TState>(_identity, mutator));
        }

        public Task<ProcessedUpdate<TState,TState>> Mutate(Func<TState,TState> mutator)
        {
            return Task.FromResult(new ProcessedUpdate<TState,TState>(_identity, mutator));
        }

        public Task<ProcessedUpdate<TState,TResult>> Mutate<TResult>(Func<TState,TResult> mutator)
            where TResult : class
        {
            return Task.FromResult(new ProcessedUpdate<TState,TResult>(_identity, mutator));
        }
    }
}
