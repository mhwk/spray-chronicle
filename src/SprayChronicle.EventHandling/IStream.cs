using System;

namespace SprayChronicle.EventHandling
{
    public interface IStream
    {
        void Read(Action<string,string,DateTime> callback);
    }
}