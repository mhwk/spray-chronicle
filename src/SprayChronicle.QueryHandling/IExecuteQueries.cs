using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IExecuteQueries
    {
        Task<object> Execute(object query);
    }
}
