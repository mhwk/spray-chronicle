using System;
using System.Linq;
using System.Collections.Generic;

namespace SprayChronicle.CommandHandling
{
    public sealed class SubscriptionCommandBus : IDispatchCommands
    {
        readonly List<IHandleCommand> handlers = new List<IHandleCommand>();

        public void Subscribe(IEnumerable<IHandleCommand> handlers)
        {
            foreach (var handler in handlers) {
                Subscribe(handler);
            }
        }

        public void Subscribe(IHandleCommand handler)
        {
            handlers.Add(handler);
        }

        public void Dispatch(object command)
        {
            try {
                handlers
                    .Where(handler => handler.Handles(command))
                    .First()
                    .Handle(command);
            } catch (Exception error) {
                throw new UnhandledCommandException(
                    string.Format(
                        "Command {0} could not be handled by one of the following handlers: {1}",
                        command.GetType(),
                        string.Join(", ", handlers.Select(h => h.GetType().Name))
                    ),
                    error
                );
            }
        }
    }
}
