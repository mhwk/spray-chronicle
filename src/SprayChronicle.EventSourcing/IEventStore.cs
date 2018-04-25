using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.EventSourcing
{
    public interface IEventStore
    {
        Task Append<T>(string identity, IEnumerable<IDomainMessage> domainMessages);

        IEventSource<T> Load<T>(string identity) where T : class;
    }
}
