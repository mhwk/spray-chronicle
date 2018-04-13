using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryRouter
    {
        Task<QueryMetadata[]> Route(object query);
    }
}
