using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public sealed class SubscriptionDispatcher : IDispatchCommands
    {
        private readonly List<IHandleCommands> _handlers = new List<IHandleCommands>();

        public void Subscribe(IEnumerable<IHandleCommands> handlers)
        {
            foreach (var handler in handlers) {
                Subscribe(handler);
            }
        }

        public void Subscribe(IHandleCommands handler)
        {
            _handlers.Add(handler);
        }

        public async Task Dispatch(params object[] commands)
        {
            await Task.Run(() => {
                foreach (var command in commands) {
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
            });
        }
    }
}
