using System;
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

        private readonly StreamOptions _streamOptions;
        
        private readonly Func<StreamOptions,Task> _initializeStream;
        
        private long _checkpoint;
        
        private bool _buffering;

        public ReadForwardSource(
            ILogger<TTarget> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            ReadForwardOptions options,
            Func<StreamOptions,Task> initializeStream) : base(logger, options.CausationId)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _initializeStream = initializeStream;
            _streamOptions = options.StreamOptions;
            _checkpoint = options.Checkpoint < 0 ? default(long) : options.Checkpoint;
        }

        protected override async Task StartBuffering()
        {
            _buffering = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _initializeStream(_streamOptions);
            
            _logger.LogDebug($"Reading forward from {_streamOptions}");

            var eos = false;
            do {
                var slice = await _eventStore.ReadStreamEventsForwardAsync(_streamOptions.TargetStream, _checkpoint, 50, false, _credentials);
                
                _logger.LogDebug($"-> Read slice {slice.FromEventNumber} to {slice.LastEventNumber}");
                
                foreach (var resolvedEvent in slice.Events) {
                    Queue.Post(resolvedEvent);
                    _checkpoint++;
                }
                eos = slice.IsEndOfStream;
            } while (!eos && _buffering);
            

            stopwatch.Stop();
            _logger.LogDebug($"Reading forward complete: {_checkpoint} events in {stopwatch.ElapsedMilliseconds}ms");

            Queue.Complete();
        }

        protected override Task StopBuffering()
        {
            _buffering = false;
            return Task.CompletedTask;
        }
    }
}
