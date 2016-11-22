using System.Collections.Generic;

namespace SprayChronicle.EventHandling
{
    public sealed class StreamHandlerManager : IManageStreamHandlers
    {
        readonly List<IHandleStream> _handlers = new List<IHandleStream>();

        public void Add(IEnumerable<IHandleStream> handlers)
        {
            foreach (var handler in handlers) {
                Add(handler);
            }
        }

        public void Add(IHandleStream handler)
        {
            _handlers.Add(handler);
        }

        public void Manage()
        {
            foreach (var handler in _handlers) {
                handler.ListenAsync();
            }
        }
    }
}
