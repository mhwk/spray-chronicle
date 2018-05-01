using System;
using System.Reflection;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class Metadata
    {
        public readonly string MessageId;

        public readonly string CausationId;

        public readonly string CorrelationId;

        public Metadata(string messageId, string causationId, string correlationId)
        {
            MessageId = messageId;
            CausationId = causationId;
            CorrelationId = correlationId;
        }
    }
}
