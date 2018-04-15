using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public abstract class CommandHandler<TSelf, TSource> : IHandle, IProcess
        where TSelf : CommandHandler<TSelf, TSource>
        where TSource : IEventSourcable<TSource>
    {
        private readonly IEventSourcingRepository<TSource> _repository;
        
        private readonly IMessageHandlingStrategy<TSelf> _handlers;
        
        private readonly IMessageHandlingStrategy<TSelf> _processors;

        protected CommandHandler(IEventSourcingRepository<TSource> repository)
            : this(
                new OverloadHandlingStrategy<TSelf>("Handle"),
                new OverloadHandlingStrategy<TSelf>("Process")
            )
        {
            _repository = repository;
        }

        protected CommandHandler(
            IMessageHandlingStrategy<TSelf> handlers,
            IMessageHandlingStrategy<TSelf> processors)
        {
            _handlers = handlers;
            _processors = processors;
        }

        protected CommandHandled<TSource,TSource> Handle(string identity)
        {
            return new CommandHandled<TSource,TSource>(identity);
        }

        protected CommandHandled<TSource,TState> Handle<TState>(string identity) where TState : TSource
        {
            return new CommandHandled<TSource,TState>(identity);
        }

        protected EventProcessed Dispatch(object command)
        {
            throw new NotImplementedException();
        }
    }
}
