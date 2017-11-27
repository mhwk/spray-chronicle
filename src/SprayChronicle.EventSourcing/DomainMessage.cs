using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public sealed class DomainMessage
    {
        public readonly long Sequence;

        public readonly DateTime Epoch;

        public readonly IMessage Payload;

        public DomainMessage(long sequence, DateTime epoch, IMessage payload)
        {
            Sequence = sequence;
            Epoch = epoch;
            Payload = payload;
        }
    }
}
