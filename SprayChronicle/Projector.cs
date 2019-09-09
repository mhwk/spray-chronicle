using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Hosting;

namespace SprayChronicle
{
    public abstract class Projector<TProject> : BackgroundService
        where TProject : IProject
    {
        private readonly IStoreEvents _events;
        private readonly BufferBlock<Envelope<object>> _queue;
        private readonly TransformBlock<Envelope<object>, Projection> _process;
        private readonly BatchBlock<Projection> _batch;
        private readonly ActionBlock<Projection[]> _commit;

        protected Projector(
            IStoreEvents events,
            IStoreSnapshots snapshots,
            TProject process,
            int batchSize
        )
        {
            _events = events;
            _queue = new BufferBlock<Envelope<object>>();
            _process = new TransformBlock<Envelope<object>, Projection>(process.Project);
            _batch = new BatchBlock<Projection>(batchSize);
            _commit = new ActionBlock<Projection[]>(Commit);

            _queue.LinkTo(_process, new DataflowLinkOptions {PropagateCompletion = true});
            _process.LinkTo(_batch, new DataflowLinkOptions {PropagateCompletion = true});
            _batch.LinkTo(_commit, new DataflowLinkOptions {PropagateCompletion = true});
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            await await Task.WhenAny(
                _commit.Completion,
                Process(cancellation)
            );
            
        }

        private async Task Process(CancellationToken cancellation)
        {
            await foreach (var envelope in _events.Watch(null, cancellation)) {
                await Process(envelope);
            }
        }

        private async Task Process(Envelope<object> evt)
        {
            await _queue.SendAsync(evt);
        }
        
        protected abstract Task Commit(Projection[] projections);
    }
}
