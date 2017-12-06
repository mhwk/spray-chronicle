using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public sealed class StreamHandler<T> : IHandleStream
        where T : IHandleEvents
    {
        private readonly ILogger<T> _logger;

        private readonly IStream _stream;

        private readonly IHandleEvents _eventsHandler;

        public StreamHandler(
            ILogger<T> logger,
            IStream stream,
            IHandleEvents eventsHandler)
        {
            _logger = logger;
            _stream = stream;
            _eventsHandler = eventsHandler;
        }

        public async Task ListenAsync()
        {
            await Task.Run(() => Listen());
        }

        public void Listen()
        {
            _stream.Subscribe((@event, at) => {
                if ( ! _eventsHandler.Processes(@event, at)) {
                    _logger.LogDebug(
                        "{0}: skipping",
                        @event.Name
                    );
                    return;
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                _eventsHandler.Process(@event, at);

                stopwatch.Stop();
                _logger.LogInformation(
                    "{0}: {1}ms",
                    @event.Name,
                    stopwatch.ElapsedMilliseconds
                );
            });
        }
    }
}
