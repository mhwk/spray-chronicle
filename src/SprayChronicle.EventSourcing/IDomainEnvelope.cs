using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public interface IDomainEnvelope : IEnvelope
    {
        long Sequence { get; }
    }
}
