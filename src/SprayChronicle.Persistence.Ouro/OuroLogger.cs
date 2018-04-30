using System;
using EventStore.ClientAPI;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroLogger : ILogger
    {
        private readonly ILogger<OuroEventStore> _logger;

        public OuroLogger(ILogger<OuroEventStore> logger)
        {
            _logger = logger;
        }

        public void Error(string format, params object[] args)
        {
            _logger.LogError(string.Format(format, args));
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            _logger.LogError(ex, string.Format(format, args));
        }

        public void Info(string format, params object[] args)
        {
            _logger.LogInformation(string.Format(format, args));
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            _logger.LogInformation(ex, string.Format(format, args));
        }

        public void Debug(string format, params object[] args)
        {
            _logger.LogDebug(string.Format(format, args));
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            _logger.LogDebug(ex, string.Format(format, args));
        }
    }
}