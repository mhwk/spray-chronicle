using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.EventSourcing
{
    public interface IEventStore
    {
        Task Append<T>(string identity, IEnumerable<IDomainEnvelope> domainMessages);

        IEventSource<T> Load<T>(string identity, string idempotencyId) where T : class;
    }
}
