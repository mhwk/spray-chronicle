using System;

namespace SprayChronicle.EventHandling
{
    public interface IStream
    {
        void OnEvent(Action<object,DateTime> callback);
    }
}