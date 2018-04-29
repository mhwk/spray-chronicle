using System;

namespace SprayChronicle.MessageHandling
{
    public interface IEnvelope
    {
        string MessageId { get; }
        
        string CausationId { get; }
        
        string CorrelationId { get; }
        
        string MessageName { get; }

        object Message { get; }
        
        DateTime Epoch { get; }
        
        Action<object> OnSuccess { get; }
        
        Action<Exception> OnError { get; }

        IEnvelope WithOnSuccess(Action<object> onSuccess);

        IEnvelope WithOnError(Action<Exception> onError);
    }
}
