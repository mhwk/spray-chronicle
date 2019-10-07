using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace SprayChronicle
{
    public abstract class Projector<TProject> : IBackgroundTask
        where TProject : IProject
    {
        private readonly ILogger<TProject> _logger;
        private readonly IStoreEvents _events;
        private readonly Timer _timer;
        private readonly BufferBlock<Envelope> _queue;
        private TransformBlock<Envelope, Envelope> _trigger;
        private readonly BatchBlock<Envelope> _batch;
        private TransformBlock<Envelope[], ProjectionResult[]> _process;
        private readonly ActionBlock<ProjectionResult[]> _commit;

        protected Projector(
            ILogger<TProject> logger,
            IStoreEvents events,
            TProject process,
            int batchSize,
            TimeSpan timeout)
        {
            _logger = logger;
            _events = events;
            _timer = new Timer(time => {
                _batch.TriggerBatch();
            });
            _queue = new BufferBlock<Envelope>();
            _trigger = new TransformBlock<Envelope,Envelope>(
                envelope => {
                    _timer.Change(timeout, Timeout.InfiniteTimeSpan);
                    return envelope;
                }
            );
            _batch = new BatchBlock<Envelope>(batchSize);
            _process = new TransformBlock<Envelope[], ProjectionResult[]>(async envelopes => {
                var time = DateTime.Now;
                try {
                    var results = new ProjectionResult[envelopes.Length];
                
                    for (var i = 0; i < envelopes.Length; i++) {
                        results[i] = new ProjectionResult {
                            Envelope = envelopes[i],
                            Projection = await process.Project(envelopes[i]),
                        };
                    }

                    return results;
                } finally {
                    _logger.LogDebug($"Processed {envelopes.Length} events in {(DateTime.Now - time).TotalMilliseconds}ms");
                }
            });
            _commit = new ActionBlock<ProjectionResult[]>(async envelopes => {
                _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                
                var time = DateTime.Now;
                try {
                    await Commit(envelopes);
                } finally {
                    _logger.LogInformation($"Committed {envelopes.Length} projections in {(DateTime.Now - time).TotalMilliseconds}ms");
                }
            });

            _queue.LinkTo(_trigger, new DataflowLinkOptions {PropagateCompletion = true});
            _trigger.LinkTo(_batch, new DataflowLinkOptions {PropagateCompletion = true});
            _batch.LinkTo(_process, new DataflowLinkOptions {PropagateCompletion = true});
            _process.LinkTo(_commit, new DataflowLinkOptions {PropagateCompletion = true});
        }

        public async Task ExecuteAsync(CancellationToken cancellation)
        {
            await await Task.WhenAny(
                _commit.Completion,
                Process(cancellation)
            );
        }

        private async Task Process(CancellationToken cancellation)
        {
            await foreach (var envelope in _events.Watch(await Checkpoint(), cancellation)) {
                await Process(envelope);
            }
        }

        protected abstract Task<Checkpoint> Checkpoint();

        private async Task Process(Envelope evt)
        {
            await _queue.SendAsync(evt);
        }
        
        protected abstract Task Commit(ProjectionResult[] results);

        public struct ProjectionResult
        {
            public Envelope Envelope { get; set; }
            public Projection Projection { get; set; }
        }
    }
}
