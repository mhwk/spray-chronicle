using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public sealed class CommandPipeline<THandler,TState> : ICommandPipeline
        where THandler : class, IHandle
        where TState : class
    {
        public string Description => $"CommandPipeline: {typeof(THandler)}";
        
        private readonly IMessageHandlingStrategy<THandler> _handlers = new OverloadHandlingStrategy<THandler>("Handle");

        private readonly IPropagatorBlock<object,object> _commands;

        public CommandPipeline(IPropagatorBlock<object,object> commands)
        {
            _commands = commands;
        }

        public Task Start()
        {
            throw new System.NotImplementedException();
        }

        public Task Stop()
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(IRouter<IHandle> router)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(IRouter<IProcess> router)
        {
            throw new System.NotImplementedException();
        }
    }
}
