using System;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryStream : IStream
    {
        readonly MemoryEventStore _eventStore;

        public MemoryStream(MemoryEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public void OnEvent(Action<object,DateTime> callback)
        {
            _eventStore.OnEventAppreared += domainMessage => callback(
                domainMessage.Payload,
                domainMessage.Epoch
            );
        }
    }
}
