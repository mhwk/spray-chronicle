using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IStoreSnapshots
    {
        Task<Snapshot> Load<TInvariant>(string invariantId, string causationId);
        
        Task Save<TInvariant>(Snapshot snapshot);
    }
}
