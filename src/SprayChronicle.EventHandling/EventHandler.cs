using System;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public abstract class EventHandler<T> : IProcessEvents where T : EventHandler<T>
    {
        private readonly IMessageHandlingStrategy<T> _eventHandlers;
    
        protected EventHandler() : this(new OverloadHandlingStrategy<T>("Process"))
        {
        }
    
        protected EventHandler(IMessageHandlingStrategy<T> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }
    
        public async Task Process(object @event, DateTime at)
        {
            await _eventHandlers.Tell(this as T, @event.ToMessage(), at);
        }
    }
}
