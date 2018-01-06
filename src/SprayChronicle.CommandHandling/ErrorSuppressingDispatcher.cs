using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public class ErrorSuppressingDispatcher : IDispatchCommands
    {
        private readonly IDispatchCommands _child;

        public ErrorSuppressingDispatcher(IDispatchCommands child)
        {
            _child = child;
        }

        public async Task Dispatch(params object[] commands)
        {
            foreach (var command in commands) {
                try {
                    await _child.Dispatch(command);
                } catch (Exception) {
                    // ignored
                }
            }
        }
    }
}