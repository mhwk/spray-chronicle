using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public abstract class CommandHandler<T> : IHandleCommands, IHandleEvents
        where T : IEventSourcable<T>
    {
        private readonly IEventSourcingRepository<T> _repository;
        
        private readonly IMessageHandlingStrategy _commandHandlers;
        
        private readonly IMessageHandlingStrategy _eventHandlers;

        protected CommandHandler(IEventSourcingRepository<T> repository)
            : this(
                repository,
                new OverloadHandlingStrategy<CommandHandler<T>>(new ContextTypeLocator<T>(), "Handle"),
                new OverloadHandlingStrategy<CommandHandler<T>>(new ContextTypeLocator<T>(), "Process")
            )
        {
        }

        protected CommandHandler(
            IEventSourcingRepository<T> repository,
            IMessageHandlingStrategy commandHandlers,
            IMessageHandlingStrategy eventHandlers)
        {
            _repository = repository;
            _commandHandlers = commandHandlers;
            _eventHandlers = eventHandlers;
        }

        protected IEventSourcingRepository<T> Repository()
        {
            return _repository;
        }

        public bool Handles(object command)
        {
            return _commandHandlers.AcceptsMessage(this, command.ToMessage());
        }

        public void Handle(object command)
        {
            try {
                _commandHandlers.ProcessMessage(this, command.ToMessage());
            } catch (TargetInvocationException error) {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            } catch (UnhandledMessageException error) {
                throw new UnhandledCommandException(
                    string.Format(
                        "Command {0} not handled by {1}",
                        command.GetType(),
                        GetType()
                    ),
                    error
                );
            }
        }

        public bool Processes(object @event, DateTime at)
        {
            return _eventHandlers.AcceptsMessage(this, @event.ToMessage(), at);
        }

        public void Process(object @event, DateTime at)
        {
            _eventHandlers.ProcessMessage(this, @event.ToMessage(), at);
        }
    }
}
