using System;

namespace SprayChronicle.MessageHandling
{
    public interface IMessage
    {
        string Name { get; }

        object Payload();

        object Payload(Type type);
    }
}
