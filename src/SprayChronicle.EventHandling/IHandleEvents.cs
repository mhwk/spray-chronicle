using System;

namespace SprayChronicle.EventHandling
{
    public interface IHandleEvents
    {
        bool Processes(object @event, DateTime at);
        void Process(object @event, DateTime at);
    }
}
