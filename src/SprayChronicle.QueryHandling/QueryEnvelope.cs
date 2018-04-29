using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public sealed class QueryEnvelope : IEnvelope
    {
        public string MessageId { get; }
        
        public string CausationId { get; }
        
        public string CorrelationId { get; }
        
        public string MessageName { get; }
        
        public object Message { get; }
        
        public DateTime Epoch { get; }
        
        public Action<object> OnSuccess { get; }
        
        public Action<Exception> OnError { get; }

        public QueryEnvelope(
            string messageId,
            string causationId,
            string correlationId,
            object query,
            DateTime epoch,
            Action<object> onSuccess,
            Action<Exception> onError)
        {
            MessageId = messageId;
            CausationId = causationId;
            CorrelationId = correlationId;
            MessageName = query.GetType().Name;
            Message = query;
            Epoch = epoch;
            OnSuccess = onSuccess;
            OnError = onError;
        }

        public IEnvelope WithOnSuccess(Action<object> onSuccess)
        {
            return new QueryEnvelope(
                MessageId,
                CausationId,
                CorrelationId,
                Message,
                Epoch,
                onSuccess,
                OnError
            );
        }

        public IEnvelope WithOnError(Action<Exception> onError)
        {
            return new QueryEnvelope(
                MessageId,
                CausationId,
                CorrelationId,
                Message,
                Epoch,
                OnSuccess,
                onError
            );
        }
    }
}
