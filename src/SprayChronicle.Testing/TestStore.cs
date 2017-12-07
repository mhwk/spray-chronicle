using System.Collections.Generic;
using System.Linq;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class TestStore : IEventStore
    {
        private readonly IEventStore _child;
        
        private readonly List<IDomainMessage> _past = new List<IDomainMessage>();

        private readonly List<IDomainMessage> _future = new List<IDomainMessage>();

        private bool _present = false;

        public TestStore(IEventStore child)
        {
            _child = child;
        }

        public void Append<T>(string identity, IEnumerable<IDomainMessage> domainMessages)
        {
            var range = domainMessages as IDomainMessage[] ?? domainMessages.ToArray();
            
            if ( ! _present) {
                _past.AddRange(range);
            } else {
                _future.AddRange(range);
            }
            
            _child.Append<T>(identity, range);
        }

        public IEnumerable<IDomainMessage> Load<T>(string identity)
        {
            return _child.Load<T>(identity);
        }

        public IEnumerable<IDomainMessage> Past()
        {
            return _past.ToArray();
        }

        public IEnumerable<IDomainMessage> Future()
        {
            return _future.ToArray();
        }

        public IEnumerable<IDomainMessage> Chronicle()
        {
            return _past.Union(_future).ToArray();
        }

        public void Present()
        {
            _present = true;
        }
    }
}
