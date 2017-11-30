using System;
using System.Linq;
using System.Collections.Generic;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryEventStore : IEventStore
    {
        private readonly Dictionary<string,List<IDomainMessage>> _streams = new Dictionary<string,List<IDomainMessage>>();

        public delegate void EventAppearedHandler(IDomainMessage domainMessage);

        public event EventAppearedHandler OnEventAppeared;

        public void Append<T>(string identity, IEnumerable<IDomainMessage> domainMessages)
        {
            CheckConcurrency(identity, domainMessages);

            foreach (var domainMessage in domainMessages) {
                Stream(identity).Add(domainMessage);
                OnEventAppeared?.Invoke(domainMessage);
            }
        }

        public IEnumerable<IDomainMessage> Load<T>(string identity)
        {
            return Stream(identity);
        }

        private List<IDomainMessage> Stream(string identity)
        {
            if ( ! _streams.ContainsKey(identity)) {
                _streams.Add(identity, new List<IDomainMessage>());
            }
            return _streams[identity];
        }

        private void CheckConcurrency(string identity, IEnumerable<IDomainMessage> domainMessages)
        {
            if (!domainMessages.Any()) {
                return;
            }
            if (domainMessages.First().Sequence != Stream(identity).Count()) {
                throw new ConcurrencyException(string.Format(
                    "Sequence {0} does not match expected {1}",
                    domainMessages.First().Sequence,
                    Stream(identity).Count()
                ));
            }
        }
    }
}
