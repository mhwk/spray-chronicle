using System;

namespace SprayChronicle.EventSourcing
{
    public sealed class DomainMessage
    {
        public readonly int Sequence;

        public readonly DateTime Epoch;

        public object Payload;

        public DomainMessage(int sequence, DateTime epoch, object payload)
        {
            Sequence = sequence;
            Epoch = epoch;
            Payload = payload;
        }
    }
}
