using System;

namespace SprayChronicle.EventHandling
{
    public interface IStream
    {
        void Subscribe(Action<object,DateTime> callback);
    }
}