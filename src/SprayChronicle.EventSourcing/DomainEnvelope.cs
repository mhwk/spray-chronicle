using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public sealed class DomainEnvelope : IDomainEnvelope
    {
        public string MessageId { get; }
        
        public string CausationId { get; }
        
        public string CorrelationId { get; }
        
        public long Sequence { get; }

        public string MessageName { get; }
        
        public object Message { get; }
        
        public DateTime Epoch { get; }
        
        public Action<object> OnSuccess { get; }
        
        public Action<Exception> OnError { get; }

        public DomainEnvelope(
            string messageId,
            string causationId,
            string correlationId,
            long sequence,
            object payload,
            DateTime epoch,
            Action<object> onSuccess = null,
            Action<Exception> onError = null)
        {
            if (null == payload) {
                throw new InvalidDomainMessageException($"You must provide a payload object (or a name, if the object is not supported by the consumer");
            }

            MessageId = messageId;
            CausationId = causationId;
            CorrelationId = correlationId;
            Sequence = sequence;
            MessageName = payload?.GetType().Name;
            Message = payload;
            Epoch = epoch;
            OnSuccess = onSuccess;
            OnError = onError;
        }
        
        public IEnvelope WithOnSuccess(Action<object> onSuccess)
        {
            return new DomainEnvelope(
                MessageId,
                CausationId,
                CorrelationId,
                Sequence,
                Message,
                Epoch,
                onSuccess,
                OnError
            );
        }

        public IEnvelope WithOnError(Action<Exception> onError)
        {
            return new DomainEnvelope(
                MessageId,
                CausationId,
                CorrelationId,
                Sequence,
                Message,
                Epoch,
                OnSuccess,
                onError
            );
        }
    }
}
