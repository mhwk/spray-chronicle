using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public class LoggingProcessor : IProcessQueries
    {
        private readonly ILogger<IProcessQueries> _logger;
        
        private readonly IMeasure _measure;

        private readonly IProcessQueries _child;

        public LoggingProcessor(
            ILogger<IProcessQueries> logger,
            IMeasure measure,
            IProcessQueries child)
        {
            _logger = logger;
            _measure = measure;
            _child = child;
        }

        public async Task<object> Process(object query)
        {
            var measurement = _measure.Start();
            
            _logger.LogDebug(
                "{0}: Processing...",
                query.GetType().Name
            );

            try {
                return await _child.Process(query);
            } catch (Exception error) {
                _logger.LogError(
                    error,
                    "{0}: Projection failure",
                    query.GetType().Name
                );
                throw;
            } finally {
                _logger.LogInformation(
                    "{0}: {1}",
                    query.GetType().Name,
                    measurement.Stop()
                );
            }
        }
    }
}
