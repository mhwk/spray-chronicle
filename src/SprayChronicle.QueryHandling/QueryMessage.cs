using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public sealed class QueryMessage : IMessage
    {
        public string Name { get; }
        
        public DateTime Epoch { get; }

        private readonly object _payload;
        
        public QueryMessage(object payload) : this(payload, DateTime.Now)
        {
        }
        
        public QueryMessage(object payload, DateTime epoch)
        {
            Name = payload.GetType().Name;
            Epoch = epoch;
            _payload = payload;
        }

        public object Payload()
        {
            return _payload;
        }
    }
}
