using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public interface IDomainMessage : IMessage
    {
        long Sequence { get; }

        DateTime Epoch { get; }
    }
}
