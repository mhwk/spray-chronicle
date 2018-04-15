using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IExecute
    {
        
    }
    
    public interface IExecute<in T> : IExecute
    {
        Task<QueryExecuted> Execute(T query);
    }
}
