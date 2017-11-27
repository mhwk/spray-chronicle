using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public interface IStream
    {
        void Subscribe(Action<IMessage,DateTime> callback);
    }
}