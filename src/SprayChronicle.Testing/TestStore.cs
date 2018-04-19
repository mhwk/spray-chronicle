using System.Collections.Generic;
using System.Linq;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class TestStore : IEventStore
    {
        private readonly IEventStore _child;
        
        private readonly EpochGenerator _epochs;

        private readonly List<IDomainMessage> _past = new List<IDomainMessage>();

        private readonly List<IDomainMessage> _future = new List<IDomainMessage>();

        private bool _present;

        public TestStore(IEventStore child, EpochGenerator epochs)
        {
            _child = child;
            _epochs = epochs;
        }

        public void Append<T>(string identity, IEnumerable<IDomainMessage> domainMessages)
        {
            var range = PrepareRange(domainMessages);
            
            if ( ! _present) {
                _past.AddRange(range);
            } else {
                _future.AddRange(range);
            }
            
            _child.Append<T>(identity, range);
        }

        private IEnumerable<IDomainMessage> PrepareRange(IEnumerable<IDomainMessage> domainMessages)
        {
            var i = 0;
            var list = new List<IDomainMessage>();
            
            foreach (var message in domainMessages) {
                if (_epochs.Count > i) {
                    list.Add(new DomainMessage(
                        message.Sequence,
                        _epochs[i],
                        message.Payload
                    ));
                } else {
                    _epochs.Add(message.Epoch);
                    list.Add(message);
                }
                i++;
            }
            
            return list.ToArray();
        }

        public IEventSource<T> Load<T>(string identity)
            where T : class
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
