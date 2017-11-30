using System;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryStream : IStream
    {
        readonly MemoryEventStore _eventStore;

        public MemoryStream(MemoryEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public void Subscribe(Action<IMessage,DateTime> callback)
        {
            _eventStore.OnEventAppeared += domainMessage => callback(
                domainMessage,
                domainMessage.Epoch
            );
        }
    }
}
