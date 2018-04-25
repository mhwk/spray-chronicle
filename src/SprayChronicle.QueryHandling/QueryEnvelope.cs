using System;

namespace SprayChronicle.QueryHandling
{
    public sealed class QueryEnvelope
    {
        public string Name { get; }

        public object Payload { get; }
        
        public Action<object> OnSuccess { get; }
        
        public Action<Exception> OnError { get; }

        public QueryEnvelope(
            object payload,
            Action<object> onSuccess,
            Action<Exception> onError)
        {
            Name = payload.GetType().Name;
            Payload = payload;
            OnSuccess = onSuccess;
            OnError = onError;
        }
    }
}
