using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class TestStore : IEventStore
    {
        private readonly IEventStore _child;
        
        private readonly EpochGenerator _epochs;

        private readonly List<IDomainEnvelope> _past = new List<IDomainEnvelope>();

        private readonly List<IDomainEnvelope> _future = new List<IDomainEnvelope>();

        private bool _present;

        public TestStore(IEventStore child, EpochGenerator epochs)
        {
            _child = child;
            _epochs = epochs;
        }

        public Task Append<T>(string identity, IEnumerable<IDomainEnvelope> domainMessages)
        {
            var range = PrepareRange(domainMessages);
            
            if ( ! _present) {
                _past.AddRange(range);
            } else {
                _future.AddRange(range);
            }
            
            _child.Append<T>(identity, range);

            return Task.CompletedTask;
        }

        private IEnumerable<IDomainEnvelope> PrepareRange(IEnumerable<IDomainEnvelope> domainMessages)
        {
            var i = 0;
            var list = new List<IDomainEnvelope>();
            
            foreach (var message in domainMessages) {
                if (_epochs.Count > i) {
                    list.Add(new DomainEnvelope(
                        Guid.NewGuid().ToString(),
                        null,
                        Guid.NewGuid().ToString(),
                        message.Sequence,
                        message.Message,
                        _epochs[i]
                    ));
                } else {
                    _epochs.Add(message.Epoch);
                    list.Add(message);
                }
                i++;
            }
            
            return list.ToArray();
        }

        public IEventSource<T> Load<T>(string identity, string causationId)
            where T : class
        {
            return _child.Load<T>(identity, causationId);
        }

        public IEnumerable<IDomainEnvelope> Past()
        {
            return _past.ToArray();
        }

        public IEnumerable<IDomainEnvelope> Future()
        {
            return _future.ToArray();
        }

        public IEnumerable<IDomainEnvelope> Chronicle()
        {
            return _past.Union(_future).ToArray();
        }

        public void Present()
        {
            _present = true;
        }
    }
}
