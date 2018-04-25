using System;

namespace SprayChronicle.QueryHandling
{
    public sealed class QueryEnvelope
    {
        public object[] Queries { get; }
        
        public Action<object> OnSuccess { get; }
        
        public Action<Exception> OnError { get; }

        public QueryEnvelope(
            object[] queries,
            Action<object> onSuccess,
            Action<Exception> onError)
        {
            Queries = queries;
            OnSuccess = onSuccess;
            OnError = onError;
        }
    }
}
