using System;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryScope
    {
    }

    public interface IQueryScope<TState,TResult,TQueryable> : IQueryScope
    {
        string Identity { get; }
        
        Func<TState,Task<TResult>>[] Mutations { get; }

        QueryMetadata<TQueryable>[] Queries { get; }
    }
}
