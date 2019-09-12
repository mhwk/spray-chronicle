using System.Collections.Generic;
using System.Threading.Tasks;

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
        
        public static async Task<Envelope<object>[]> ToSync(this IAsyncEnumerable<Envelope<object>> envelopes)
        {
            var list = new List<Envelope<object>>();
            
            await foreach (var envelope in envelopes) {
                list.Add(envelope);
            }

            return list.ToArray();
        }
    }
}
