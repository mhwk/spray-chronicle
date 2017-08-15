using System;

namespace SprayChronicle.EventHandling
{
    public interface IHandleEvent
    {
        
    }

    public interface IHandleEvent<in T> : IHandleEvent
    {
        void On(T @event, DateTime epoch);
    }
}
