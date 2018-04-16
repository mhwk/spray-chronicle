using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public class LoggingRouter : IRouter<IExecute>
    {
        private readonly ILogger<IRouter<IExecute>> _logger;
        
        private readonly IMeasure _measure;

        private readonly IRouter<IExecute> _child;

        public LoggingRouter(
            ILogger<IRouter<IExecute>> logger,
            IMeasure measure,
            IRouter<IExecute> child)
        {
            _logger = logger;
            _measure = measure;
            _child = child;
        }

        public IRouter<IExecute> Subscribe(IMessageHandlingStrategy<IExecute> strategy, HandleMessage handler)
        {
            _child.Subscribe(strategy, handler);
            return this;
        }

        public IRouter<IExecute> Subscribe(IRouterSubscriber<IExecute> subscriber)
        {
            _child.Subscribe(subscriber);
            return this;
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
