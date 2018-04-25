using System;
using System.Threading.Tasks;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class ProcessedUpdate<TSource,TTarget> : Processed
        where TSource : class
        where TTarget : class
    {
        private readonly Func<TSource, TTarget> _mutation;
        
        public ProcessedUpdate(string identity, Func<TSource,TTarget> mutation): base(identity)
        {
            _mutation = mutation;
        }

        internal override Task<object> Do(object state = null)
        {
            if (null == state) {
                throw new ArgumentException($"State is expected to be {typeof(TSource)}, null given");
            }

            if (!(state is TSource source)) {
                throw new ArgumentException($"State {state.GetType()} is not assignable to {typeof(TSource)}");
            }
            
            return Task.FromResult<object>(
                _mutation?.Invoke(source)
            );
        }
    }
}
