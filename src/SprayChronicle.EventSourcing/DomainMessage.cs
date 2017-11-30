using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public sealed class DomainMessage : IDomainMessage
    {
        public long Sequence { get; }
        
        public DateTime Epoch { get; }
        
        public string Name { get; }
        
        private readonly object _payload;

        public DomainMessage(long sequence, DateTime epoch, object payload)
        {
            Sequence = sequence;
            Epoch = epoch;
            Name = payload.GetType().Name;
            _payload = payload;
        }

        public object Payload()
        {
            return _payload;
        }

        public object Payload(Type type)
        {
            if (!_payload.GetType().IsAssignableFrom(type)) {
                throw new IncompatibleMessageException(string.Format(
                    "Message {0} is not convertable to {1}",
                    _payload.GetType(),
                    type
                ));
            }
            return _payload;
        }
    }
}
