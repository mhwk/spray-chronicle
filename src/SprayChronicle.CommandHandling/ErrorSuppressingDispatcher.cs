using System;

namespace SprayChronicle.CommandHandling
{
    public class ErrorSuppressingDispatcher : IDispatchCommand
    {
        private readonly LoggingDispatcher _internalDispatcher;

        public ErrorSuppressingDispatcher(LoggingDispatcher internalDispatcher)
        {
            _internalDispatcher = internalDispatcher;
        }

        public void Dispatch(object command)
        {
            try {
                _internalDispatcher.Dispatch(command);
            } catch (Exception) {
                // ignored
            }
        }
    }
}