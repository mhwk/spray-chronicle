using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class EventSourcedFixture<TModule> : IPopulate, IExecute where TModule : IModule, new()
    {
        IContainer _container;

        long _sequence = -1;

        public EventSourcedFixture(): this(builder => {})
        {}

        public EventSourcedFixture(Action<ContainerBuilder> configure)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<SprayChronicle.CommandHandling.CommandHandlingModule>();
            builder.RegisterModule<TModule>();

            builder.Register<ILoggerFactory>(c => new LoggerFactory().AddConsole(LogLevel.Debug));
            builder
                .Register<EventSourcedTestStore>(c => new EventSourcedTestStore())
                .AsSelf()
                .As<IEventStore>()
                .SingleInstance();

            configure(builder);
            _container = builder.Build();
        }

		public IExecute Given(params object[] messages)
        {
            _container.Resolve<EventSourcedTestStore>().History(messages.Select(payload => new DomainMessage(
                ++_sequence,
                new DateTime(),
                payload
            )).ToArray());

            return this;
        }

		public IValidate When(object message)
        {
            Exception e = null;
            try {
                _container.Resolve<LoggingCommandBus>().Dispatch(message);
            } catch (UnhandledCommandException error) {
                e = error.InnerException;
            } catch (Exception error) {
                e = error;
            }
            return new EventSourcedValidator(e, _container.Resolve<EventSourcedTestStore>().Future());
        }
    }
}
