using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.QueryHandling
{
    public class LoggingQueryProcessor : IProcessQueries
    {
        readonly ILogger<IProcessQueries> _logger;

        readonly IProcessQueries _innerProcessor;

        public LoggingQueryProcessor(ILogger<IProcessQueries> logger, IProcessQueries innerProcessor)
        {
            _logger = logger;
            _innerProcessor = innerProcessor;
        }

        public object Process(object query)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try {
                return _innerProcessor.Process(query);
            } catch (Exception) {
                throw;
            } finally {
                stopwatch.Stop();
                _logger.LogInformation("[{0}] {1}ms", query.GetType(), stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
