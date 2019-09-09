using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IStoreEvents
    {
        IAsyncEnumerable<Envelope<object>> Load(DateTime? since, CancellationToken cancellation);
        
        IAsyncEnumerable<Envelope<object>> Watch(DateTime? since, CancellationToken cancellation);

        IAsyncEnumerable<Envelope<object>> Load<TInvariant>(string invariantId, string causationId, long fromPosition);
        
        Task Append<TInvariant>(IEnumerable<Envelope<object>> envelopes);
    }
}
