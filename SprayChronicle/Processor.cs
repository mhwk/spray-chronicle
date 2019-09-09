using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SprayChronicle
{
    public sealed class Processor<TProcess> : BackgroundService
        where TProcess : IProcess
    {
        private readonly ILogger<TProcess> _logger;
        private readonly IStoreEvents _events;
        private readonly BufferBlock<Envelope<object>> _queue;
        private readonly ActionBlock<Envelope<object>> _process;

        public Processor(
            ILogger<TProcess> logger,
            IStoreEvents events,
            TProcess process
        )
        {
            _logger = logger;
            _events = events;
            _queue = new BufferBlock<Envelope<object>>();
            _process = new ActionBlock<Envelope<object>>(async e => {
                try {
                    await process.Process(e);
                } catch (IdempotencyException error) {
                    _logger.LogDebug($"{error}");
                }
            });

            _queue.LinkTo(_process, new DataflowLinkOptions {PropagateCompletion = true});
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            try {
                await await Task.WhenAny(
                    _process.Completion,
                    Process(cancellation)
                );
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception error) {
                _logger.LogCritical($"The process was shut-down due to an error: {error}");
            }
        }

        private async Task Process(CancellationToken cancellation)
        {
            await foreach (var envelope in _events.Watch(null, cancellation)) {
                await Process(envelope);
            }
        }

        private async Task Process(Envelope<object> envelope)
        {
            await _queue.SendAsync(envelope);
        }
    }
}
