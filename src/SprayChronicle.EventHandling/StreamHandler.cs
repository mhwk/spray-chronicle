using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SprayChronicle.Server;

namespace SprayChronicle.EventHandling
{
    public sealed class StreamHandler<T> : IHandleStream
        where T : IHandleEvents
    {
        private readonly ILogger<T> _logger;
        
        private readonly IMeasure _measure;

        private readonly IStream _stream;

        private readonly IHandleEvents _handler;

        public StreamHandler(
            ILogger<T> logger,
            IMeasure measure,
            IStream stream,
            IHandleEvents handler)
        {
            _logger = logger;
            _measure = measure;
            _stream = stream;
            _handler = handler;
        }

        public Task ListenAsync()
        {
            Task.Factory.StartNew(
                Listen,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            ).ConfigureAwait(false);
            
            return Task.CompletedTask;
        }

        public void Listen()
        {
            _stream.Subscribe((message, at) => {
                if ( ! _handler.Processes(message, at)) {
                    _logger.LogDebug(
                        "{0}: Skipping...",
                        message.Name
                    );
                    return;
                }

                var measurement = _measure.Start();

                _handler.Process(message, at);

                _logger.LogInformation(
                    "{0}: {1}",
                    message.Name,
                    measurement.Stop()
                );
            });
        }
    }
}
