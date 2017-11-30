using System.Collections.Generic;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class TestStore : IEventStore
    {
        private readonly List<DomainMessage> _history = new List<DomainMessage>();

        private readonly List<DomainMessage> _future = new List<DomainMessage>();

        public void Append<T>(string identity, IEnumerable<DomainMessage> domainMesages)
        {
            _future.AddRange(domainMesages);
        }

        public IEnumerable<DomainMessage> Load<T>(string identity)
        {
            return _history.ToArray();
        }

        public void History(IEnumerable<DomainMessage> domainMessages)
        {
            _history.AddRange(domainMessages);
        }

        public IEnumerable<DomainMessage> Future()
        {
            return _future.ToArray();
        }
    }
}
