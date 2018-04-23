using System;

namespace SprayChronicle.EventSourcing
{
    public sealed class DomainMessage : IDomainMessage
    {
        public long Sequence { get; }
        
        public DateTime Epoch { get; }
        
        public string Name { get; }
        
        public object Payload { get; }

        public DomainMessage(long sequence, DateTime epoch, object payload)
        {
            if (null == payload) {
                throw new InvalidDomainMessageException($"You must provide a payload object (or a name, if the object is not supported by the consumer");
            }
            Sequence = sequence;
            Epoch = epoch;
            Name = payload?.GetType().Name;
            Payload = payload;
        }

        public DomainMessage(long sequence, DateTime epoch, string name)
        {
            Sequence = sequence;
            Epoch = epoch;
            Name = name;
            Payload = new object();
        }
    }
}
