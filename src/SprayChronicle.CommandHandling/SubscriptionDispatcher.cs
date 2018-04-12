using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

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

        public SubscriptionDispatcher Subscribe(IHandleCommands handler)
        {
            _handlers.Add(handler);

            return this;
        }

        public async Task Dispatch(params object[] commands)
        {
            foreach (var command in commands) {
                var handler = _handlers.FirstOrDefault(e => MessageHandlingMetadata.Accepts(e.GetType(), command.GetType()));

                if (null == handler) {
                    throw new UnhandledCommandException(
                        string.Format(
                            "Command {0} could not be handled by one of the following handlers: {1}",
                            command.GetType(),
                            string.Join(", ", _handlers.Select(h => h.GetType().Name))
                        )
                    );
                }

                await handler.Handle(command);
            }
        }
    }
}
