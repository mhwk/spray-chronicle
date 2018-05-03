using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public interface IEventEnvelope : IEnvelope
    {
        long Sequence { get; }
    }
}
