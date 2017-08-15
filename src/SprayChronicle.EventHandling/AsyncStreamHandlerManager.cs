using System.Threading.Tasks;
using System.Collections.Generic;

namespace SprayChronicle.EventHandling
{
    public sealed class AsyncStreamHandlerManager : IManageStreamHandlers
    {
        private readonly List<IHandleStream> _handlers = new List<IHandleStream>();

        private readonly List<Task> _tasks = new List<Task>();

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
                _tasks.Add(handler.ListenAsync());
            }
        }
    }
}
