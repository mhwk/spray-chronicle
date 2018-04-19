using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class ReadForwardSource<TTarget> : OuroSource<TTarget>
        where TTarget : class
    {
        private readonly ILogger<TTarget> _logger;
        
        private readonly IEventStoreConnection _eventStore;
        
        private readonly UserCredentials _credentials;

        private readonly string _streamName;
        
        private long _checkpoint;
        
        private bool _running;

        public ReadForwardSource(
            ILogger<TTarget> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            ReadForwardOptions options)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = options.StreamName;
            _checkpoint = options.Checkpoint;
        }

        protected override async Task StartBuffering()
        {
            _running = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var eos = false;
            do {
                var slice = await _eventStore.ReadStreamEventsForwardAsync(_streamName, _checkpoint, 50, false, _credentials);
                foreach (var resolvedEvent in slice.Events) {
                    Queue.Post(resolvedEvent);
                    _checkpoint++;
                }
                eos = slice.IsEndOfStream;
            } while (!eos && _running);
            
            Queue.Complete();

            stopwatch.Stop();
            _logger.LogDebug("[{0}::load] {1}ms", _streamName, stopwatch.ElapsedMilliseconds);
            
        }

        protected override Task StopBuffering()
        {
            _running = false;
            return Task.CompletedTask;
        }
    }
}
