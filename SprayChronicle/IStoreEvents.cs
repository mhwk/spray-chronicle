using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IStoreEvents
    {
        IAsyncEnumerable<Envelope> Load(long? checkpoint, CancellationToken cancellation);
        
        IAsyncEnumerable<Envelope> Watch(long? checkpoint, CancellationToken cancellation);

        IAsyncEnumerable<Envelope> Load<TInvariant>(string invariantId, string causationId, long fromPosition);
        
        Task Append<TInvariant>(IEnumerable<Envelope> envelopes);
    }
}
