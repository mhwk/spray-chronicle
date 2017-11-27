using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public sealed class StreamEventHandler<T> : IHandleStream where T: IHandleEvent
    {
        private readonly ILogger<T> _logger;

        private readonly IStream _stream;

        private readonly IHandleEvent _eventHandler;

        private static readonly IMessageHandlingStrategy Handlers = new OverloadHandlingStrategy<T>(new ContextTypeLocator<T>());

        public StreamEventHandler(
            ILogger<T> logger,
            IStream stream,
            IHandleEvent eventHandler)
        {
            _logger = logger;
            _stream = stream;
            _eventHandler = eventHandler;
        }

        public async Task ListenAsync()
        {
            await Task.Run(() => Listen());
        }

        public void Listen()
        {
            _stream.Subscribe((@event, occurrence) => {
                if ( ! Handlers.AcceptsMessage(_eventHandler, @event, occurrence)) {
                    _logger.LogDebug(
                        "{0}: skipping",
                        @event.GetType().Name
                    );
                    return;
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Handlers.ProcessMessage(_eventHandler, @event, occurrence);

                stopwatch.Stop();
                _logger.LogInformation(
                    "{0}: {1}ms",
                    @event.GetType().Name,
                    stopwatch.ElapsedMilliseconds
                );
            });
        }
    }
}
