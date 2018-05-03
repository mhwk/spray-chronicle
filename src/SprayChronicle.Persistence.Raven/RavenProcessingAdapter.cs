using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenProcessingAdapter<TProcessor,TState> : IQueryProcessingAdapter
        where TProcessor : RavenQueries<TProcessor,TState>
        where TState : class
    {
        private readonly ILogger<TProcessor> _logger;

        private readonly IDocumentStore _store;
        
        private readonly string _checkpointName;

        private long _checkpoint;

        public RavenProcessingAdapter(
            ILogger<TProcessor> logger,
            IDocumentStore store,
            string checkpointName = null)
        {
            _logger = logger;
            _store = store;
            _checkpointName = checkpointName ?? typeof(TState).Name;
        }
        
        public async Task Apply(Processed[] processed)
        {
            var ravenProcessed = processed.Cast<RavenProcessed>().ToArray();
            
            using (var session = _store.OpenAsyncSession()) {
                var identities = ravenProcessed
                    .Where(p => p != null)
                    .Select(p => p.Identity)
                    .Distinct()
                    .ToArray();
            
                var documents = await session.LoadAsync<TState>(identities);
                
                foreach (var process in ravenProcessed) {
                    if (null == process) {
//                        _logger.LogDebug($"Skipping null at {i}");
                        continue;
                    }
                    
                    documents[process.Identity] = (TState) await process.Do(documents[process.Identity]);
                    if (null == documents[process.Identity]) {
                        throw new Exception($"Null value has been set");
                    }

                    var cancellation = new CancellationTokenSource();
                    
                    await session.StoreAsync(
                        documents[process.Identity],
                        process.Identity,
                        cancellation.Token
                    );
                }

                await MarkCheckpoint(session, _checkpoint += processed.Length);
            
                await session.SaveChangesAsync();
            }
        }

        public async Task<long> Checkpoint()
        {
            using (var session = _store.OpenAsyncSession()) {
                var id = $"Checkpoint/{_checkpointName}";
                var checkpoint = await session.LoadAsync<Checkpoint>(id);
    
                if (null == checkpoint) {
                    _logger.LogDebug("Starting from the beginning");
                    return -1;
                }
                
                _logger.LogDebug($"Starting from checkpoint {checkpoint.Sequence}");
                return checkpoint.Sequence;
            }
        }

        private async Task MarkCheckpoint(IAsyncDocumentSession session, long sequence)
        {
            var id = $"Checkpoint/{_checkpointName}";
            
            var checkpoint = await session.LoadAsync<Checkpoint>(id);
            if (null != checkpoint) {
                checkpoint.Increase(sequence);
            } else {
                checkpoint = new Checkpoint(id);
                await session.StoreAsync(checkpoint);
            }
            
            _logger.LogDebug($"Checkpoint {id} at {sequence}");
        }
    }
}
