using System.Collections.Generic;

namespace SprayChronicle.EventSourcing
{
    public interface IEventStore
    {
        void Append<T>(string identity, IEnumerable<IDomainMessage> domainMessages);

        IEventSource<T> Load<T>(string identity) where T : class;
    }
}
