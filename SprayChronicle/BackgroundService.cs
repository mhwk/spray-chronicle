using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SprayChronicle
{
    public sealed class BackgroundService<T> : BackgroundService
        where T : IBackgroundTask
    {
        private readonly ILogger<T> _logger;
        private readonly T _task;

        public BackgroundService(
            ILogger<T> logger,
            T task
        )
        {
            _logger = logger;
            _task = task;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try {
                await _task.ExecuteAsync(stoppingToken);
            } catch (Exception error) {
                _logger.LogError(error.ToString());
                throw;
            }
        }
    }
}
