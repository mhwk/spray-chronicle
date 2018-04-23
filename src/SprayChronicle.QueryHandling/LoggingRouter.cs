using System;
using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public class LoggingRouter : IMessageRouter
    {
        private readonly ILogger<IExecute> _logger;
        
        private readonly IMeasure _measure;

        private readonly IMessageRouter _child;

        public LoggingRouter(
            ILogger<IExecute> logger,
            IMeasure measure,
            IMessageRouter child)
        {
            _logger = logger;
            _measure = measure;
            _child = child;
        }

        public Task<object> Route(params object[] arguments)
        {
            var measurement = _measure.Start();
            var argumentList = string.Join(", ", arguments.Select(m => m.GetType().Name));

            try {
                _logger.LogDebug(
                    $"{argumentList}: Executing..."
                );

                return _child.Route(arguments);
            } catch (Exception error) {
                _logger.LogError(
                    error,
                    $"{argumentList}: Execution failure"
                );
                throw;
            } finally {
                _logger.LogInformation(
                    $"{argumentList}: {measurement.Stop()}"
                );
            }
        }
    }
}
