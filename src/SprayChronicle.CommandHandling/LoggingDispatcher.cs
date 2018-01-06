using System;
using System.Threading.Tasks;
using SprayChronicle.Server;

namespace SprayChronicle.CommandHandling
{
    public class LoggingDispatcher : IDispatchCommands
    {
        private readonly ILogger<IDispatchCommands> _logger;
        
        private readonly IMeasure _measure;

        private readonly IDispatchCommands _child;

        public LoggingDispatcher(ILogger<IDispatchCommands> logger, IMeasure measure, IDispatchCommands child)
        {
            _logger = logger;
            _measure = measure;
            _child = child;
        }

        public async Task Dispatch(params object[] commands)
        {
            foreach (var command in commands) {
                var measure = _measure.Start();
                
                _logger.LogDebug("{0}: Dispatching...", command.GetType().Name);

                try {
                    await _child.Dispatch(command);
                } catch (UnhandledCommandException error) {
                    _logger.LogWarning(
                        error,
                        "{0}: Not handled",
                        command.GetType().Name
                    );
                    throw;
                } catch (Exception error) {
                    _logger.LogDebug(
                        error,
                        "{0}: Domain exception",
                        command.GetType().Name
                    );
                    throw;
                } finally {
                    _logger.LogInformation("{0}: {1}", command.GetType().Name, measure);
                }
            }
        }
    }
}
