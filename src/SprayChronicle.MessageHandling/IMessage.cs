using System;

namespace SprayChronicle.MessageHandling
{
    public interface IMessage
    {
        string Type { get; }

        object Instance();

        object Instance(Type type);
    }
}
