using System;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.CommandHandling
{
    public class LoggingCommandBus : IDispatchCommands
    {
        readonly ILogger _logger;

        readonly IDispatchCommands _internalDispatcher;

        public LoggingCommandBus(ILogger logger, IDispatchCommands internalDispatcher)
        {
            _logger = logger;
            _internalDispatcher = internalDispatcher;
        }

        public void Dispatch(object command)
        {
            try {
                _internalDispatcher.Dispatch(command);
            } catch (Exception error) {
                _logger.LogWarning("Dispatch of command failed", error);
            }
        }
    }
}
