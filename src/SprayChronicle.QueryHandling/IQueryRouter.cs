using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryRouter
    {
        Task<object> Route(object query);
    }
}
