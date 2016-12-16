using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public sealed class StreamEventHandler<T> : IHandleStream where T: IHandleEvent
    {
        readonly ILogger<T> _logger;

        readonly IStream _stream;

        readonly IHandleEvent _eventHandler;

        static readonly IMessageHandlingStrategy _handlers = new OverloadHandlingStrategy<T>();

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
            _stream.OnEvent((@event, occurrence) => {
                if ( ! _handlers.AcceptsMessage(_eventHandler, @event, occurrence)) {
                    _logger.LogDebug(
                        "{0}: skipping",
                        @event.GetType().Name
                    );
                    return;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                _handlers.ProcessMessage(_eventHandler, @event, occurrence);

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
