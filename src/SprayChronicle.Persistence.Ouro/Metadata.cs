using System;
using System.Reflection;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class Metadata
    {
        public readonly string OriginalFqn;

        public readonly string MessageId;

        public readonly string CausationId;

        public readonly string CorrelationId;

        public Metadata(Type originalFqn, string messageId, string causationId, string correlationId)
        {
            OriginalFqn = $"{originalFqn}, {originalFqn.GetTypeInfo().Assembly}";
            MessageId = messageId;
            CausationId = causationId;
            CorrelationId = correlationId;
        }
    }
}
