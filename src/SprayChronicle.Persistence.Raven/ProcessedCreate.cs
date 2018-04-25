using System;
using System.Threading.Tasks;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class ProcessedCreate<TState> : Processed
        where TState : class
    {
        private readonly Func<TState> _mutation;
        
        public ProcessedCreate(Func<TState> mutation): base(null)
        {
            _mutation = mutation;
        }

        internal override Task<object> Do(object state = null)
        {
            if (null != state) {
                throw new ArgumentException($"State is expected to be null, {state.GetType()} given");
            }

            return Task.FromResult<object>(_mutation?.Invoke());
        }
    }
}