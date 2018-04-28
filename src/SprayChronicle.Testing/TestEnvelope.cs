using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.Testing
{
    public sealed class TestEnvelope : IEnvelope
    {
        public string MessageId { get; }
        public string CausationId { get; }
        public string CorrelationId { get; }
        public string MessageName { get; }
        public object Message { get; }
        public DateTime Epoch { get; }
        
        public TestEnvelope()
        {
            MessageId = Guid.NewGuid().ToString();
            CausationId = null;
            CorrelationId = Guid.NewGuid().ToString();
            MessageName = typeof(object).Name;
            Message = new object();
            Epoch = DateTime.Now;
        }
        
        public TestEnvelope(object payload)
        {
            MessageId = Guid.NewGuid().ToString();
            CausationId = null;
            CorrelationId = Guid.NewGuid().ToString();
            MessageName = payload.GetType().Name;
            Message = payload;
            Epoch = DateTime.Now;
        }
    }
}
