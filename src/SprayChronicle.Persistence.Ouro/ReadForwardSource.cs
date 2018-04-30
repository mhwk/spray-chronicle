using System;
using System.Diagnostics;
using System.Threading;
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
        
        private bool _buffering;

        public ReadForwardSource(
            ILogger<TTarget> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            ReadForwardOptions options) : base(logger, options.CausationId)
        {
            Console.WriteLine($"----------------------{typeof(TTarget).Name} - {options.CausationId}");
            Thread.Sleep(1);
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = options.StreamName;
            _checkpoint = options.Checkpoint;
        }

        protected override async Task StartBuffering()
        {
            _buffering = true;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            _logger.LogDebug($"Reading forward from {_streamName}");

            var eos = false;
            do {
                var slice = await _eventStore.ReadStreamEventsForwardAsync(_streamName, _checkpoint, 50, false, _credentials);
                
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
