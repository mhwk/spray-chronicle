using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryExecutionAdapter
    {
        Task<object> Apply(Executed executed);
    }
}
