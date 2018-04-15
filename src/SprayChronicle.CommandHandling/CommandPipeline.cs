using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public sealed class CommandPipeline<THandler,TState> : ICommandRouterSubscriber, IProcess
        where THandler : class, IHandle
        where TState : class
    {
        private readonly IMessageHandlingStrategy<THandler> _handlers = new OverloadHandlingStrategy<THandler>("Handle");

        private readonly IPropagatorBlock<object,object> _commands;

        public CommandPipeline(IPropagatorBlock<object,object> commands)
        {
            _commands = commands;
        }

        public void Subscribe(SubscriptionRouter router)
        {
            _handlers.EachType(type => {
                router.Subscribe(type, command => {
                    _commands.Post(command);
                    return Task.CompletedTask;
                });
            });
        }
    }
}
