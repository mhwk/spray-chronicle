using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public abstract class OverloadCommandHandler<T> : IHandleCommand where T : IEventSourcable<T>
    {
        protected readonly IEventSourcingRepository<T> _repository;

        static IMessageHandlingStrategy _handlers = new OverloadHandlingStrategy<OverloadCommandHandler<T>>(); 

        public OverloadCommandHandler(IEventSourcingRepository<T> repository)
        {
            _repository = repository;
        }

        public bool Handles(object command)
        {
            return _handlers.AcceptsMessage(this, command);
        }

        public void Handle(object command)
        {
            try {
                _handlers.ProcessMessage(this, command);
            } catch (TargetInvocationException error) {
                ExceptionDispatchInfo.Capture(error).Throw();
            } catch (Exception error) {
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

        protected void Start<TResult>(Func<TResult> callback) where TResult : T
        {
            _repository.Save<TResult>(callback());
        }

        protected void Continue<TResult>(string identity, Func<TResult,TResult> callback) where TResult : T
        {
            _repository.Save<TResult>(callback(_repository.Load<TResult>(identity)));
        }

        protected void Continue<TInit,TResult>(string identity, Func<TInit,TResult> callback) where TInit : T where TResult : T
        {
            _repository.Save<TResult>(callback(_repository.Load<TInit>(identity)));
        }
    }
}
