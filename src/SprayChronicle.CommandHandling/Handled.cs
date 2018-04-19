using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public abstract class Handled
    {
        public string Identity { get; }

        protected Handled(string identity)
        {
            Identity = identity;
        }

        internal abstract Task<object> Do(object sourcable = null);
    }
}
