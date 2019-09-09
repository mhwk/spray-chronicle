using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace SprayChronicle
{
    public abstract class Invariant<TInvariant> :
        IArrange<TInvariant>,
        IAct<TInvariant>
        where TInvariant : Invariant<TInvariant>
    {
        private ImmutableList<object> _scheduled = ImmutableList<object>.Empty;

        public IEnumerable<object> Commit()
        {
            try {
                return _scheduled;
            } finally {
                _scheduled = ImmutableList<object>.Empty;
            }
        }

        protected TInvariant Apply(params object[] evts)
        {
            var invariant = this;

            foreach (var evt in evts) {
                invariant = Arrange(evt);
            }

            invariant._scheduled = _scheduled.AddRange(evts);

            return (TInvariant)invariant;
        }

        public abstract TInvariant Arrange(object evt);

        public abstract Task<TInvariant> Act(object cmd);
    }
}
