using System.Threading.Tasks;
using SprayChronicle.EventHandling;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryProcessingAdapter
    {
        Task Apply(Processed[] processed);

        Task<long> Checkpoint();
    }
}