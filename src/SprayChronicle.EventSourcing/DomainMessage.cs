using System;
using SprayChronicle.MessageHandling;

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
            Sequence = sequence;
            Epoch = epoch;
            Name = payload.GetType().Name;
            Payload = payload;
        }
    }
}
