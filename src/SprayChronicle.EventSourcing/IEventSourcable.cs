using System.Collections.Generic;

namespace SprayChronicle.EventSourcing
{
    public interface IEventSourcable<out TSelf> where TSelf : IEventSourcable<TSelf>
    {
        string Identity();

        IEnumerable<IDomainMessage> Diff();
    }
}
