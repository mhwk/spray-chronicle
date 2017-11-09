using System;
using System.Linq;
using System.Collections.Generic;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryEventStore : IEventStore
    {
        private readonly Dictionary<string,List<DomainMessage>> _streams = new Dictionary<string,List<DomainMessage>>();

        public delegate void EventAppearedHandler(DomainMessage domainMessage);

        public event EventAppearedHandler OnEventAppeared;

        public void Append<T>(string identity, IEnumerable<DomainMessage> domainMessages)
        {
            CheckConcurrency(identity, domainMessages);

            foreach (var domainMessage in domainMessages) {
                Stream(identity).Add(domainMessage);
                OnEventAppeared?.Invoke(domainMessage);
            }
        }

        public IEnumerable<DomainMessage> Load<T>(string identity)
        {
            return Stream(identity);
        }

        private List<DomainMessage> Stream(string identity)
        {
            if ( ! _streams.ContainsKey(identity)) {
                _streams.Add(identity, new List<DomainMessage>());
            }
            return _streams[identity];
        }

        private void CheckConcurrency(string identity, IEnumerable<DomainMessage> domainMessages)
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
