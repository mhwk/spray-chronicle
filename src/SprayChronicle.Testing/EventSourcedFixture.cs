using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Testing
{
    public class EventSourcedFixture<TModule,TSourced> : Fixture<TModule,TSourced,TSourced> where TModule : IModule, new() where TSourced : EventSourced<TSourced>
    {
        private long _sequence = -1;

        private TSourced _sourced;

        public EventSourcedFixture(): this(builder => {})
        {}

        public EventSourcedFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<CommandHandlingModule>();
                builder.RegisterModule<MemoryModule>();
                builder.Register<ILoggerFactory>(c => new LoggerFactory().AddConsole(LogLevel)).SingleInstance();
                builder
                    .Register(c => new TestStore(c.Resolve<MemoryEventStore>()))
                    .AsSelf()
                    .As<IEventStore>()
                    .SingleInstance();
                configure(builder);
            })
        {
        }

		public override IExecute<TSourced,TSourced> Given(params object[] messages)
        {
            _sourced = EventSourced<TSourced>.Patch(messages.Select(payload => new DomainMessage(
                ++_sequence,
                new DateTime(),
                payload
            )).ToArray());

            return this;
        }

		public override async Task<IValidate> When(Func<TSourced,TSourced> callback)
        {
            return await EventSourcedValidator.Run(Container, () => callback(_sourced));
        }
    }
}
