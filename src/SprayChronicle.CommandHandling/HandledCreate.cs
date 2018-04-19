using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public sealed class HandledCreate<TState> : Handled
        where TState : class
    {
        private readonly Func<Task<TState>> _mutation;
        
        public HandledCreate(Func<Task<TState>> mutation) : base(null)
        {
            _mutation = mutation;
        }

        internal override Task<object> Do(object sourcable = null)
        {
            if (null != sourcable) {
                throw new ArgumentException($"Sourcable is expected to be null, {sourcable.GetType()} given");
            }

            return Task.FromResult<object>(
                _mutation?.Invoke()
            );
        }
    }
}
