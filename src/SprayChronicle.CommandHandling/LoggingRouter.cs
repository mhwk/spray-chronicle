using System;
using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.CommandHandling
{
    public class LoggingRouter : IRouter<IHandle>
    {
        private readonly ILogger<IRouter<IHandle>> _logger;
        
        private readonly IMeasure _measure;

        private readonly IRouter<IHandle> _child;

        public LoggingRouter(ILogger<IRouter<IHandle>> logger, IMeasure measure, IRouter<IHandle> child)
        {
            _logger = logger;
            _measure = measure;
            _child = child;
        }

        public IRouter<IHandle> Subscribe(IMessageHandlingStrategy<IHandle> strategy, HandleMessage handler)
        {
            _child.Subscribe(strategy, handler);
            return this;
        }

        public IRouter<IHandle> Subscribe(IRouterSubscriber<IHandle> subscriber)
        {
            _child.Subscribe(subscriber);
            return this;
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
