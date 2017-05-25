using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.CommandHandling
{
    public class LoggingCommandBus : IDispatchCommands
    {
        readonly ILogger<LoggingCommandBus> _logger;

        readonly IDispatchCommands _internalDispatcher;

        public LoggingCommandBus(ILogger<LoggingCommandBus> logger, IDispatchCommands internalDispatcher)
        {
            _logger = logger;
            _internalDispatcher = internalDispatcher;
        }

        public void Dispatch(object command)
        {
            try {
                _logger.LogDebug("Dispatching: " + command.GetType().ToString());

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                _internalDispatcher.Dispatch(command);

                stopwatch.Stop();
                _logger.LogInformation("{0}: {1}ms", command.GetType().ToString(), stopwatch.ElapsedMilliseconds);
            } catch (UnhandledCommandException error) {
                _logger.LogWarning(string.Format(
                    "Command {0} not handled: {1}",
                    command.GetType().ToString(),
                    error.ToString()
                ));
                throw;
            } catch (Exception error) {
                _logger.LogDebug(string.Format(
                    "Command {0} triggered domain exception: {1}",
                    command.GetType().ToString(),
                    error.ToString()
                ));
                throw;
            }
        }
    }
}
