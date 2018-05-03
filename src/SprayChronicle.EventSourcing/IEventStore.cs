using System.Collections.Generic;
using System.Threading.Tasks;
using SprayChronicle.EventHandling;

namespace SprayChronicle.EventSourcing
{
    public interface IEventStore
    {
        Task Append<T>(string identity, IEnumerable<IEventEnvelope> domainMessages);

        IEventSource<T> Load<T>(string identity, string causationId) where T : class;
    }
}
