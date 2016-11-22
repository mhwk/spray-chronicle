using System.Collections.Generic;
using System.Linq;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public sealed class TestRepository<T> : IObjectRepository<T> where T : EventSourced<T>
    {
        IEnumerable<DomainMessage> _history = new List<DomainMessage>();

        IEnumerable<DomainMessage> _future = new List<DomainMessage>();

        public void History(params DomainMessage[] history)
        {
            _history = history;
        }

        public void History(IEnumerable<DomainMessage> history)
        {
            _history = history;
        }

        public IEnumerable<DomainMessage> Future()
        {
            return _future;
        }

        public void Save(T sourced)
        {
            _future = sourced.Diff();

            if (0 < _history.Count() && _future.First().Sequence != _history.Last().Sequence + 1) {
                throw new ConcurrencyException(string.Format(
                    "Sequence {0} does not match expected version {1}",
                    _future.First().Sequence,
                    _history.Last().Sequence + 1
                ));
            }

            if (0 == _history.Count() && _future.First().Sequence != 0) {
                throw new ConcurrencyException(string.Format(
                    "Sequence {0} does not match expected version 0",
                    _future.First().Sequence
                ));
            }
        }

        public void Save<TChild>(T sourced) where TChild : T
        {
            if ( ! (sourced is TChild)) {
                throw new InvalidStateException(string.Format(
                    "Expected state {0}, but got {1}",
                    typeof(TChild),
                    sourced.GetType()
                ));
            }
            Save((T) sourced);
        }

        public T Load(string identity)
        {
            return EventSourced<T>.Patch(_history);
        }

        public TChild Load<TChild>(string identity) where TChild : T
        {
            var sourced = Load(identity);
            if ( ! (sourced is TChild)) {
                throw new InvalidStateException(string.Format(
                    "Expected state {0}, but got {1}",
                    typeof(TChild),
                    sourced.GetType()
                ));
            }
            return (TChild) sourced;
        }
    }
}
