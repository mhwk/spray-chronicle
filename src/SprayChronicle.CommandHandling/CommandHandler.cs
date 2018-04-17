using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.CommandHandling
{
    public abstract class CommandHandler<TSelf, TState> : IHandle, IProcess
        where TSelf : CommandHandler<TSelf, TState>
        where TState : class, IEventSourcable<TState>
    {
        protected HandledFactory<TState> Handle()
        {
            return new HandledFactory<TState>();
        }
        
        protected HandledFactory<TState,TState> Handle(string identity)
        {
            return new HandledFactory<TState,TState>(identity);
        }

        protected HandledFactory<TStart,TState> Handle<TStart>(string identity) where TStart : class
        {
            return new HandledFactory<TStart,TState>(identity);
        }

        protected ProcessedFactory Process()
        {
            return new ProcessedFactory();
        }
    }
}
