using System;

namespace SprayChronicle.MessageHandling
{
    public interface IMessage
    {
        string Name { get; }
        
        DateTime Epoch { get; }

        object Payload();
    }
}
