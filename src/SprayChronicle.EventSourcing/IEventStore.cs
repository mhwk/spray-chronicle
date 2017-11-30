using System.Collections.Generic;

namespace SprayChronicle.EventSourcing
{
    public interface IEventStore
    {
        void Append<T>(string identity, IEnumerable<IDomainMessage> domainMessages);

        IEnumerable<IDomainMessage> Load<T>(string identity);
    }
}
