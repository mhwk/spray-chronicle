using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public abstract class EventHandler<T> : IHandleEvents
        where T : class
    {
        private readonly IMessageHandlingStrategy _eventHandlers;
        
        protected EventHandler()
            : this(new OverloadHandlingStrategy<T>(new ContextTypeLocator<T>(), "Process"))
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
