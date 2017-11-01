using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Testing
{
    public class EventSourcedFixture<TModule> : IPopulate, IExecute where TModule : IModule, new()
    {
        IContainer _container;

        long _sequence = -1;

        protected LogLevel _logLevel = LogLevel.Information;

        public EventSourcedFixture(): this(builder => {})
        {}

        public EventSourcedFixture(Action<ContainerBuilder> configure)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<TModule>();
            builder.RegisterModule<CommandHandlingModule>();
            builder.RegisterModule<SyncEventHandlingModule>();
            builder.RegisterModule<MemoryModule>();
            builder.RegisterModule<ProjectingModule>();
            builder.RegisterModule<QueryHandlingModule>();

            builder.Register<ILoggerFactory>(c => new LoggerFactory().AddConsole(_logLevel));
            builder
                .Register<EventSourcedTestStore>(c => new EventSourcedTestStore())
                .AsSelf()
                .As<IEventStore>()
                .SingleInstance();

            configure(builder);
            _container = builder.Build();
            _container.Resolve<IManageStreamHandlers>().Manage();
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
                _container.Resolve<LoggingDispatcher>().Dispatch(message);
            } catch (UnhandledCommandException error) {
                e = error.InnerException;
            } catch (Exception error) {
                e = error;
            }
            return new EventSourcedValidator(e, _container.Resolve<EventSourcedTestStore>().Future());
        }
    }
}
