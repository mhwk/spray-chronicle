using System.Threading.Tasks;

namespace SprayChronicle.Persistence.Raven
{
    public abstract class RavenProcessed : EventHandling.Processed
    {
        public string Identity { get; }

        protected RavenProcessed(string identity)
        {
            Identity = identity;
        }

        internal abstract Task<object> Do(object state = null);
    }
}
