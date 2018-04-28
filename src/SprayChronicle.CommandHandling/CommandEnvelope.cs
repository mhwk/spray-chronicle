using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public sealed class CommandEnvelope : IEnvelope
    {
        public string MessageId { get; }
        
        public string CausationId { get; }
        
        public string CorrelationId { get; }
        
        public string MessageName { get; }
        
        public object Message { get; }
        
        public DateTime Epoch { get; }
        
        public Action OnSuccess { get; }
        
        public Action<Exception> OnError { get; }

        public CommandEnvelope(
            string messageId,
            string causationId,
            string correlationId,
            object command,
            DateTime epoch,
            Action onSuccess,
            Action<Exception> onError)
        {
            if (null == command) {
                throw new InvalidCommandException(
                    "Command can not be null",
                    messageId,
                    causationId,
                    correlationId
                );
            }
            
            MessageId = messageId;
            CausationId = causationId;
            CorrelationId = correlationId;
            MessageName = command.GetType().Name;
            Message = command;
            Epoch = epoch;
            OnSuccess = onSuccess;
            OnError = onError;
        }

    }
}
