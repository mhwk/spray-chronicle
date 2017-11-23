using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public abstract class CommandHandler<T> : IHandleCommand where T : IEventSourcable<T>
    {
        protected readonly IEventSourcingRepository<T> _repository;

        protected CommandHandler(IEventSourcingRepository<T> repository)
        {
            _repository = repository;
        }
        
        public abstract bool Handles(object command);
        
        public abstract void Handle(object command);

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

        protected void Continue<TInit,TResult>(Func<TInit> load, Func<TInit,TResult> callback) where TInit : T where TResult : T
        {
            _repository.Save<TResult>(callback(load()));
        }
    }
}
