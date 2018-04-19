using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public sealed class HandledFactory<TState>
        where TState : class
    {
        public Task<HandledCreate<TState>> Mutate(Func<Task<TState>> mutation)
        {
            return Task.FromResult(new HandledCreate<TState>(mutation));
        }
    }
    
    public sealed class HandledFactory<TStart,TState>
        where TStart : class
        where TState : class
    {
        private readonly string _identity;

        public HandledFactory(string identity)
        {
            _identity = identity;
        }
        
        public Task<HandledUpdate<TStart,TState>> Mutate(Func<TStart,Task<TState>> mutation)
        {
            return Task.FromResult(new HandledUpdate<TStart,TState>(_identity, mutation));
        }
    }
}
