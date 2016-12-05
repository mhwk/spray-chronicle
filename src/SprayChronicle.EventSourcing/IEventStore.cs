using System.Collections.Generic;

namespace SprayChronicle.EventSourcing
{
    public interface IEventStore
    {
        void Append<T>(string identity, IEnumerable<DomainMessage> domainMessages);

        IEnumerable<DomainMessage> Load<T>(string identity);
    }
}
