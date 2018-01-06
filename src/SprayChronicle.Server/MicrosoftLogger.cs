using System;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.Server
{
    public sealed class MicrosoftLogger<T> : ILogger<T>
    {
        private readonly Microsoft.Extensions.Logging.ILogger<T> _logger;

        public MicrosoftLogger(Microsoft.Extensions.Logging.ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogDebug(Exception error)
        {
            _logger.LogDebug(error.ToString());
        }

        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        public void LogDebug(Exception error, string message)
        {
            _logger.LogDebug(error, message);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        public void LogDebug(Exception error, string message, params object[] args)
        {
            _logger.LogDebug(error, message, args);
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(Exception error)
        {
            _logger.LogWarning(error.ToString());
        }

        public void LogWarning(Exception error, string message)
        {
            _logger.LogWarning(error, message);
        }

        public void LogWarning(Exception error, string message, params object[] args)
        {
            _logger.LogWarning(error, message, args);
        }

        public void LogError(Exception error)
        {
            _logger.LogError(error.ToString());
        }

        public void LogError(Exception error, string message)
        {
            _logger.LogError(error, message);
        }

        public void LogError(Exception error, string message, params object[] args)
        {
            _logger.LogError(error, message, args);
        }

        public void LogCritical(Exception error)
        {
            _logger.LogCritical(error.ToString());
        }

        public void LogCritical(Exception error, string message)
        {
            _logger.LogCritical(error, message);
        }

        public void LogCritical(Exception error, string message, params object[] args)
        {
            _logger.LogCritical(error, message, args);
        }
    }
}
