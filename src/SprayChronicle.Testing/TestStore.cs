using System.Linq;
using System.Collections.Generic;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class TestStore : IEventStore
    {
        readonly List<DomainMessage> _stream = new List<DomainMessage>();

        readonly List<DomainMessage> _history = new List<DomainMessage>();
        
        public void Append<T>(string identity, IEnumerable<DomainMessage> domainMesages)
        {
            _stream.AddRange(domainMesages);
        }

        public IEnumerable<DomainMessage> Load<T>(string identity)
        {
            return _stream.ToArray();
        }

        public void Record()
        {
            _history.AddRange(_stream);
        }

        public IEnumerable<DomainMessage> Recorded(string identity)
        {
            return _stream.Except(_history);
        }
    }
}
