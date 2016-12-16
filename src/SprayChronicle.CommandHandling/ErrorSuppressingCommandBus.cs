using System;

namespace SprayChronicle.CommandHandling
{
    public class ErrorSuppressingCommandBus : IDispatchCommands
    {
        readonly LoggingCommandBus _internalDispatcher;

        public ErrorSuppressingCommandBus(LoggingCommandBus internalDispatcher)
        {
            _internalDispatcher = internalDispatcher;
        }

        public void Dispatch(object command)
        {
            try {
                _internalDispatcher.Dispatch(command);
            } catch (Exception) {}
        }
    }
}