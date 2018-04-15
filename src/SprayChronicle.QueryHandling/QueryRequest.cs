using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public sealed class QueryRequest : IMessage
    {
        public string Name { get; }

        public object Payload { get; }
        
        public DateTime Epoch { get; }
        
        public Action<object> OnSuccess { get; }
        
        public QueryRequest(object payload, Action<object> onSuccess) : this(payload, DateTime.Now, onSuccess)
        {
        }
        
        public QueryRequest(object payload, DateTime epoch, Action<object> onSuccess)
        {
            Name = payload.GetType().Name;
            Epoch = epoch;
            Payload = payload;
            OnSuccess = onSuccess;
        }
    }
}
