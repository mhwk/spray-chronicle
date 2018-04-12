using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public abstract class CommandHandler<TSelf, TSource> : IHandleCommands, IProcessEvents
        where TSelf : CommandHandler<TSelf, TSource>
        where TSource : IEventSourcable<TSource>
    {
        private readonly IEventSourcingRepository<TSource> _repository;
        
        private readonly IMessageHandlingStrategy<TSelf> _commandHandlers;
        
        private readonly IMessageHandlingStrategy<TSelf> _eventHandlers;

        protected CommandHandler(IEventSourcingRepository<TSource> repository)
            : this(
                new OverloadHandlingStrategy<TSelf>("Handle"),
                new OverloadHandlingStrategy<TSelf>("Process")
            )
        {
            _repository = repository;
        }

        protected CommandHandler(
            IMessageHandlingStrategy<TSelf> commandHandlers,
            IMessageHandlingStrategy<TSelf> eventHandlers)
        {
            _commandHandlers = commandHandlers;
            _eventHandlers = eventHandlers;
        }

        public async Task Handle(object command)
        {
            try {
                await _commandHandlers.Tell(this as TSelf, command.ToMessage());
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

        public async Task Process(object @event, DateTime at)
        {
            await _eventHandlers.Tell(this as TSelf, @event.ToMessage(), at);
        }

        protected Scope<TSource> For(string identity)
        {
            return new Scope<TSource>(_repository, identity);
        }

        protected Scope<TState> For<TState>(string identity) where TState : TSource
        {
            return new Scope<TState>(_repository, identity);
        }

        protected sealed class Scope<TState> where TState : TSource
        {
            private readonly IEventSourcingRepository<TSource> _repository;
            
            private readonly string _identity;

            public Scope(IEventSourcingRepository<TSource> repository, string identity)
            {
                _repository = repository;
                _identity = identity;
            }

            public async Task Do(Func<TSource> mutate)
            {
                await _repository.Save(
                    mutate()
                );
            }
            
            public async Task Do(Func<TState,TSource> mutate)
            {
                await _repository.Save(
                    mutate(
                        await _repository.LoadOrDefault<TState>(_identity)
                    )
                );
            }
        }
    }
}
