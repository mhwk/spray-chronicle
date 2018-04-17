using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            try {
                _logger.LogDebug(
                    "{0}: Executing...",
                    string.Join(", ", arguments.Select(m => m.GetType().Name))
                );

                return _child.Route(arguments);
            } catch (Exception error) {
                _logger.LogError(
                    error,
                    "{0}: Execution failure",
                    string.Join(", ", arguments.Select(m => m.GetType().Name))
                );
                throw;
            } finally {
                _logger.LogInformation(
                    "{0}: {1}",
                    string.Join(", ", arguments.Select(m => m.GetType().Name)),
                    measurement.Stop()
                );
            }
        }
    }
}
