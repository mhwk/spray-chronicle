using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public class ErrorSuppressingDispatcher : IDispatchCommands
    {
        private readonly LoggingDispatcher _internalDispatcher;

        public ErrorSuppressingDispatcher(LoggingDispatcher internalDispatcher)
        {
            _internalDispatcher = internalDispatcher;
        }

        public async Task Dispatch(object command)
        {
            await Task.Run(async () => {
                try {
                    await _internalDispatcher.Dispatch(command);
                } catch (Exception) {
                    // ignored
                }
            });
        }
    }
}