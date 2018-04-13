using System;
using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryExecutor
    {
        
    }
    
    public interface IQueryExecutor<in T> : IQueryExecutor
    {
        Task<QueryMetadata> Execute(T query);
    }
}
