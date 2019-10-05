using System;

namespace SprayChronicle
{
    public sealed class Envelope
    {
        public string MessageId { get; private set; }
        public string CausationId { get; private set; }
        public string CorrelationId { get; private set; }
        public string InvariantId { get; private set; }
        public string InvariantType { get; private set; }
        public long Sequence { get; private set; }
        public object Message { get; private set; }
        public DateTime Epoch { get; private set; }

        public Envelope(
            string invariantId,
            string invariant,
            long sequence,
            object message
        ) : this(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            invariantId,
            invariant,
            sequence,
            message,
            DateTime.Now
        )
        {
        }

        private Envelope(
            string messageId,
            string causationId,
            string correlationId,
            string invariantId,
            string invariantType,
            long sequence,
            object message,
            DateTime epoch)
        {
            MessageId = messageId;
            CausationId = causationId;
            CorrelationId = correlationId;
            InvariantId = invariantId;
            InvariantType = invariantType;
            Sequence = sequence;
            Message = message;
            Epoch = epoch;
        }

        public Envelope CausedBy(string causationId)
        {
            return new Envelope(
                MessageId,
                causationId,
                CorrelationId,
                InvariantId,
                InvariantType,
                Sequence,
                Message,
                Epoch
            );
        }

        public Envelope CorrelatesTo(string correlationId)
        {
            return new Envelope(
                MessageId,
                CausationId,
                correlationId,
                InvariantId,
                InvariantType,
                Sequence,
                Message,
                Epoch
            );
        }
    }
}
