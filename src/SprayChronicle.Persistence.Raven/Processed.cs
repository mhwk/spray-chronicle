using System.Threading.Tasks;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class Processed : EventHandling.Processed
    {
        public string Identity { get; }

        protected Processed(string identity)
        {
            Identity = identity;
        }

        internal abstract Task<object> Do(object state = null);
    }
}
