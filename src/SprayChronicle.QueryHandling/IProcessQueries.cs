using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IProcessQueries
    {
        Task<object> Process(object query);
    }
}
