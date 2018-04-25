using System;

namespace SprayChronicle.CommandHandling
{
    public sealed class CommandEnvelope
    {
        public object Command { get; }
        
        public Action OnSuccess { get; }
        
        public Action<Exception> OnError { get; }

        public CommandEnvelope(
            object command,
            Action onSuccess,
            Action<Exception> onError)
        {
            Command = command;
            OnSuccess = onSuccess;
            OnError = onError;
        }
    }
}
