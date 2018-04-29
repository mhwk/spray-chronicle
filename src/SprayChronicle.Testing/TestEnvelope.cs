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
        public Action<object> OnSuccess { get; }
        public Action<Exception> OnError { get; }

        public TestEnvelope()
        {
            MessageId = Guid.NewGuid().ToString();
            CausationId = null;
            CorrelationId = Guid.NewGuid().ToString();
            MessageName = typeof(object).Name;
            Message = new object();
            Epoch = DateTime.Now;
            OnSuccess = result => { };
            OnError = error => { };
        }
        
        public TestEnvelope(object payload)
        {
            MessageId = Guid.NewGuid().ToString();
            CausationId = null;
            CorrelationId = Guid.NewGuid().ToString();
            MessageName = payload.GetType().Name;
            Message = payload;
            Epoch = DateTime.Now;
            OnSuccess = result => { };
            OnError = error => { };
        }

        private TestEnvelope(
            string messageId,
            string causationId,
            string correlationId,
            object message,
            DateTime epoch,
            Action<object> onSuccess,
            Action<Exception> onError)
        {
            MessageId = messageId;
            CausationId = causationId;
            CorrelationId = correlationId;
            Message = message;
            Epoch = epoch;
            OnSuccess = onSuccess;
            OnError = onError;
        }

        public IEnvelope WithOnSuccess(Action<object> onSuccess)
        {
            return new TestEnvelope(
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
            return new TestEnvelope(
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
