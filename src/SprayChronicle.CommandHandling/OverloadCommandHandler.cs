using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public abstract class OverloadCommandHandler<T> : CommandHandler<T> where T : IEventSourcable<T>
    {
        private static readonly IMessageHandlingStrategy Handlers = new OverloadHandlingStrategy<OverloadCommandHandler<T>>(new ContextTypeLocator<T>());

        protected OverloadCommandHandler(IEventSourcingRepository<T> repository) : base(repository)
        {}

        public override bool Handles(object command)
        {
            return Handlers.AcceptsMessage(this, command.ToMessage());
        }

        public override void Handle(object command)
        {
            try {
                Handlers.ProcessMessage(this, command.ToMessage());
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
    }
}
