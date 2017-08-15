using System.Collections.Generic;

namespace SprayChronicle.EventSourcing
{
    public interface IEventSourcable<T> where T : IEventSourcable<T>
    {
        string Identity();

        IEnumerable<DomainMessage> Diff();
    }
}
