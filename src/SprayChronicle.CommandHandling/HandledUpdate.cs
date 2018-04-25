using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public abstract class HandledUpdate<TState> : Handled
        where TState : class
    {
        protected HandledUpdate(string identity) : base(identity)
        {
        }
        
        internal abstract Task<TState> Do(object sourcable = null);
    }
    
    public class HandledUpdate<TStart,TState> : HandledUpdate<TState>
        where TStart : class
        where TState : class
    {
        private readonly Func<TStart, Task<TState>> _mutation;
        
        public HandledUpdate(string identity, Func<TStart,Task<TState>> mutation) : base(identity)
        {
            _mutation = mutation;
        }

        internal override async Task<TState> Do(object sourcable = null)
        {
            if (null == sourcable) {
                throw new ArgumentException($"Sourcable is expected to be {typeof(TStart)} but was null");
            }

            if (!(sourcable is TStart state)) {
                throw new ArgumentException($"Sourcable is expected to be {typeof(TStart)} but was {typeof(TState)}");
            }

            return await _mutation.Invoke(state);
        }
    }
}
