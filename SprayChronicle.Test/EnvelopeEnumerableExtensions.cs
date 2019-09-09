using System.Collections.Generic;

namespace SprayChronicle.Test
{
    public static class EnvelopeEnumerableExtensions
    {
        public static async IAsyncEnumerable<Envelope<object>> ToAsync(this IEnumerable<Envelope<object>> envelopes)
        {
            foreach (var envelope in envelopes) {
                yield return envelope;
            }
        }
    }
}
