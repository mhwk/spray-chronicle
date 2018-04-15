using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public abstract class CommandHandled
    {
        
    }
    
    public sealed class CommandHandled<TSource,TState> : CommandHandled where TState : TSource
    {
        private readonly string _identity;

        public CommandHandled(string identity)
        {
            _identity = identity;
        }

        public Task<CommandHandled> Mutate(Func<TSource> mutate)
        {
                
        }
            
        public Task<CommandHandled> Mutate(Func<TState,TSource> mutate)
        {
                
        }
    }
}
