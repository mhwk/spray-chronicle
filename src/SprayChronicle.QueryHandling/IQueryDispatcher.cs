using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryDispatcher
    {
        Task<object> Dispatch(object query);
    }
}
