using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.CommandHandling
{
    public class LoggingDispatcher : IDispatchCommands
    {
        private readonly ILogger<LoggingDispatcher> _logger;

        private readonly IDispatchCommands _internalDispatcher;

        public LoggingDispatcher(ILogger<LoggingDispatcher> logger, IDispatchCommands internalDispatcher)
        {
            _logger = logger;
            _internalDispatcher = internalDispatcher;
        }

        public async Task Dispatch(object command)
        {
            await Task.Run(async () => {
                try {
                    _logger.LogDebug("Dispatching: " + command.GetType());

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    await _internalDispatcher.Dispatch(command);

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
            });
        }
    }
}
