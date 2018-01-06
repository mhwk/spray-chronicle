using Microsoft.Extensions.Logging;

namespace SprayChronicle.Server
{
    public sealed class MicrosoftLoggerFactory : ILoggerFactory
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _factory;

        public MicrosoftLoggerFactory(Microsoft.Extensions.Logging.ILoggerFactory factory)
        {
            _factory = factory;
        }

        public ILogger<T> Create<T>()
        {
            return new MicrosoftLogger<T>(
                _factory.CreateLogger<T>()
            );
        }
    }
}
