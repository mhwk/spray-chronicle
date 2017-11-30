using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.QueryHandling
{
    public class LoggingQueryProcessor : IProcessQueries
    {
        private readonly ILogger<IProcessQueries> _logger;

        private readonly IProcessQueries _innerProcessor;

        public LoggingQueryProcessor(ILogger<IProcessQueries> logger, IProcessQueries innerProcessor)
        {
            _logger = logger;
            _innerProcessor = innerProcessor;
        }

        public async Task<object> Process(object query)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                return await _innerProcessor.Process(query);
            } catch (Exception) {
                throw;
            } finally {
                stopwatch.Stop();
                _logger.LogInformation("[{0}] {1}ms", query.GetType(), stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
