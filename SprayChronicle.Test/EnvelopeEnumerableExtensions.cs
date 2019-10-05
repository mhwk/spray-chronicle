using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.Test
{
    public static class EnvelopeEnumerableExtensions
    {
        public static async IAsyncEnumerable<Envelope> ToAsync(this IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes) {
                yield return envelope;
            }
        }
        
        public static async Task<Envelope[]> ToSync(this IAsyncEnumerable<Envelope> envelopes)
        {
            var list = new List<Envelope>();
            
            await foreach (var envelope in envelopes) {
                list.Add(envelope);
            }

            return list.ToArray();
        }
    }
}
