using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public class ErrorSuppressingRouter : ICommandRouter
    {
        private readonly ICommandRouter _child;

        public ErrorSuppressingRouter(ICommandRouter child)
        {
            _child = child;
        }

        public async Task Route(params object[] commands)
        {
            foreach (var command in commands) {
                try {
                    await _child.Route(command);
                } catch (Exception) {
                    // ignored
                }
            }
        }
    }
}