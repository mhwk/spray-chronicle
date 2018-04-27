using System;
using System.Threading.Tasks;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class ProcessedUpdate<TState,TTarget> : Processed
        where TState : class
        where TTarget : class
    {
        private readonly Func<TState, TTarget> _mutation;
        
        public ProcessedUpdate(string identity, Func<TState,TTarget> mutation): base($"{typeof(TState).Name}/{identity}")
        {
            _mutation = mutation;
        }

        internal override Task<object> Do(object state = null)
        {
            if (null == state) {
                throw new ArgumentException($"State is expected to be {typeof(TState)}, null given");
            }

            if (!(state is TState source)) {
                throw new ArgumentException($"State {state.GetType()} is not assignable to {typeof(TState)}");
            }
            
            return Task.FromResult<object>(
                _mutation?.Invoke(source)
            );
        }
    }
}
