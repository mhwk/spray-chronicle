﻿using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public class HandledUpdate<TSource,TTarget> : Handled
        where TSource : class
        where TTarget : class
    {
        private readonly Func<TSource, Task<TTarget>> _mutation;
        
        public HandledUpdate(string identity, Func<TSource,Task<TTarget>> mutation) : base(identity)
        {
            _mutation = mutation;
        }

        internal override async Task<object> Do(object sourcable = null)
        {
            if (null == sourcable) {
                throw new ArgumentException($"Sourcable is expected to be {typeof(TSource)}, null given");
            }

            if (!(sourcable is TSource state)) {
                throw new ArgumentException($"Sourcable {sourcable.GetType()} is not assignable to {typeof(TSource)}");
            }

            return Task.FromResult<object>(
                await _mutation.Invoke(state)
            );
        }
    }
}