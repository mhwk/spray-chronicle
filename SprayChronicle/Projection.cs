using System;

namespace SprayChronicle
{
    public abstract class Projection
    {
        public sealed class None : Projection
        {
            
        }

        public sealed class Mutate<TState> : Projection
        {
            public Type Type => typeof(TState);
            public string Identity { get; }
            public Func<TState,TState> Mutator { get; }

            public Mutate(string identity, Func<TState,TState> mutator)
            {
                Identity = identity;
                Mutator = mutator;
            }

            public TState DoMutate(TState state)
            {
                return Mutator(state);
            }
        }
    }
}
