using System;
using System.Threading.Tasks;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenProcessorFactory<TState>
        where TState : class
    {
        private readonly string _identity;

        public RavenProcessorFactory()
        {
        }

        public RavenProcessorFactory(string identity)
        {
            _identity = identity;
        }

        public Task<RavenProcessedCreate<TState>> Mutate(Func<TState> mutator)
        {
            return Task.FromResult(new RavenProcessedCreate<TState>(mutator));
        }

        public Task<RavenProcessedUpdate<TState,TState>> Mutate(Func<TState,TState> mutator)
        {
            return Task.FromResult(new RavenProcessedUpdate<TState,TState>(_identity, mutator));
        }

        public Task<RavenProcessedUpdate<TState,TResult>> Mutate<TResult>(Func<TState,TResult> mutator)
            where TResult : class
        {
            return Task.FromResult(new RavenProcessedUpdate<TState,TResult>(_identity, mutator));
        }
    }
}
