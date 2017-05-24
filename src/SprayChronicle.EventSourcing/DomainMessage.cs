using System;

namespace SprayChronicle.EventSourcing
{
    public sealed class DomainMessage
    {
        public readonly long Sequence;

        public readonly DateTime Epoch;

        public object Payload;

        public DomainMessage(long sequence, DateTime epoch, object payload)
        {
            Sequence = sequence;
            Epoch = epoch;
            Payload = payload;
        }
    }
}
