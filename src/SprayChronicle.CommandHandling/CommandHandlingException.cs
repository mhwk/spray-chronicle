using System;

namespace SprayChronicle.CommandHandling
{
    public abstract class CommandHandlingException : Exception
    {
        protected CommandHandlingException(string message, string messageId, string causationId, string correlationId)
            : base($"{message}\n  message id: {messageId}\n  causation id: {causationId}\n  correlation id: {correlationId}")
        {
        }
    }
}
