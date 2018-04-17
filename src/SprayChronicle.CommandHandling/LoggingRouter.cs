using System;
using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.CommandHandling
{
    public class LoggingRouter : IMessageRouter
    {
        private readonly ILogger<IHandle> _logger;
        
        private readonly IMeasure _measure;

        private readonly IMessageRouter _child;

        public LoggingRouter(ILogger<IHandle> logger, IMeasure measure, IMessageRouter child)
        {
            _logger = logger;
            _measure = measure;
            _child = child;
        }

        public async Task<object> Route(params object[] arguments)
        {
            var measure = _measure.Start();
            var argList = string.Join(", ", arguments.Select(a => a.GetType().ToString()));
            
            _logger.LogDebug($"{argList}: Dispatching...");

            try {
                await _child.Route(arguments);
                return null;
            } catch (UnhandledCommandException error) {
                _logger.LogWarning(
                    error,
                    $"{argList}: Not handled"
                );
                throw;
            } catch (Exception error) {
                _logger.LogDebug(
                    error,
                    $"{argList}: Domain exception"
                );
                throw;
            } finally {
                _logger.LogInformation($"{argList}: {measure}");
            }
        }
    }
}
