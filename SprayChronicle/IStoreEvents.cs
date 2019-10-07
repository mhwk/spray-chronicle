using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IStoreEvents
    {
        IAsyncEnumerable<Envelope> Load(Checkpoint? checkpoint, CancellationToken cancellation);
        
        IAsyncEnumerable<Envelope> Watch(Checkpoint? checkpoint, CancellationToken cancellation);

        IAsyncEnumerable<Envelope> Load<TInvariant>(string invariantId, string causationId, long fromPosition);
        
        Task Append<TInvariant>(IEnumerable<Envelope> envelopes);
    }
}
