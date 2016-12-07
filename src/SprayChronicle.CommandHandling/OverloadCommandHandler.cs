using System;
using System.Linq;
using System.Reflection;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.CommandHandling
{
    public abstract class OverloadCommandHandler<T> : IHandleCommand where T : IEventSourcable<T>
    {
        protected readonly IEventSourcingRepository<T> _repository;

        public OverloadCommandHandler(IEventSourcingRepository<T> repository)
        {
            _repository = repository;
        }

        MethodInfo LocateMethodFor(object command)
        {
            return GetType().GetTypeInfo().GetMethods()
                .Where(m => m.GetParameters().Length > 0)
                .Where(m => m.GetParameters()[0].ParameterType.Equals(command.GetType()))
                .FirstOrDefault();
        }

        public bool Handles(object command)
        {
            return null != LocateMethodFor(command);
        }

        public void Handle(object command)
        {
            MethodInfo method = LocateMethodFor(command);
            if (null == method) {
                throw new UnhandledCommandException(string.Format(
                    "Command {0} not handled by {1}",
                    command.GetType(),
                    GetType()
                ));
            }
            try {
                method.Invoke(this, new object[] { command });
            } catch (TargetInvocationException error) {
                throw error.InnerException;
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
