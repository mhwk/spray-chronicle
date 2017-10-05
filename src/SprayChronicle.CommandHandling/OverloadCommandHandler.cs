using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public abstract class OverloadCommandHandler<T> : CommandHandler<T> where T : IEventSourcable<T>
    {
        static readonly IMessageHandlingStrategy _handlers = new OverloadHandlingStrategy<OverloadCommandHandler<T>>();

        protected OverloadCommandHandler(IEventSourcingRepository<T> repository) : base(repository)
        {}

        public override bool Handles(object command)
        {
            return _handlers.AcceptsMessage(this, command);
        }

        public override void Handle(object command)
        {
            try {
                _handlers.ProcessMessage(this, command);
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
