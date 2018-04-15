using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryExecuted
    {
        public Action<object> OnSuccess { get; }
    }
    
    public abstract class QueryExecuted<TSession> : QueryExecuted
    {
        public abstract Task<object> Do(TSession queryable);
    }
}
