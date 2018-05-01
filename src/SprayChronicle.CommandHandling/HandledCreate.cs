using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public sealed class HandledCreate<TState> : Handled
        where TState : class
    {
        private readonly Func<Task<TState>> _mutation;
        
        public HandledCreate(string identity, Func<Task<TState>> mutation) : base(identity)
        {
            _mutation = mutation;
        }

        internal Task<TState> Do(object sourcable = null)
        {
            if (null != sourcable) {
                throw new ArgumentException($"Sourcable is expected to be null, {sourcable.GetType()} given");
            }

            return _mutation?.Invoke();
        }
    }
}
