using System.Threading.Tasks;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class RavenProcessed : Processed
    {
        public string Identity { get; }

        protected RavenProcessed(string identity)
        {
            Identity = identity;
        }

        internal abstract Task<object> Do(object state = null);
    }
}
