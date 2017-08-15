using System.Linq;
using System.Collections.Generic;

namespace SprayChronicle.CommandHandling
{
    public sealed class SubscriptionDispatcher : IDispatchCommand
    {
        private readonly List<IHandleCommand> _handlers = new List<IHandleCommand>();

        public void Subscribe(IEnumerable<IHandleCommand> handlers)
        {
            foreach (var handler in handlers) {
                Subscribe(handler);
            }
        }

        public void Subscribe(IHandleCommand handler)
        {
            _handlers.Add(handler);
        }

        public void Dispatch(object command)
        {
            var handler = _handlers.FirstOrDefault(h => h.Handles(command));
            
            if (null == handler) {
                throw new UnhandledCommandException(
                    string.Format(
                        "Command {0} could not be handled by one of the following handlers: {1}",
                        command.GetType(),
                        string.Join(", ", _handlers.Select(h => h.GetType().Name))
                    )
                );
            }

            handler.Handle(command);
        }
    }
}
