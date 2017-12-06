using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public abstract class EventHandler : IHandleEvents
    {
        private readonly IMessageHandlingStrategy _eventHandlers;
        
        protected EventHandler()
        {
        }
        
        protected EventHandler(IMessageHandlingStrategy eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public bool Processes(object @event, DateTime at)
        {
            return _eventHandlers.AcceptsMessage(this, @event.ToMessage(), at);
        }

        public void Process(object @event, DateTime at)
        {
            _eventHandlers.ProcessMessage(this, @event.ToMessage(), at);
        }
    }
}
