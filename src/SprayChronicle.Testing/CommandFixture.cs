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
    public sealed class CommandFixture<TModule> : Fixture<TModule,IDispatchCommand,Task> where TModule : IModule, new()
    {
        public CommandFixture(): this(builder => {})
        {}

        public CommandFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<CommandHandlingModule>();
                builder.RegisterModule<SyncEventHandlingModule>();
                builder.RegisterModule<MemoryModule>();
                builder.RegisterModule<ProjectingModule>();
                builder.RegisterModule<QueryHandlingModule>();
                builder
                    .Register<TestStore>(c => new TestStore())
                    .AsSelf()
                    .As<IEventStore>()
                    .SingleInstance();
                configure(builder);
            })
        {
        }
        
        protected override void Boot()
        {
            Container.Resolve<IManageStreamHandlers>().Manage();
        }

		public override IExecute<IDispatchCommand,Task> Given(params object[] messages)
		{
		    foreach (var message in messages) {
		        Container.Resolve<ErrorSuppressingDispatcher>().Dispatch(message).Wait();
		    }

            return this;
        }

        public override IValidate<Task> When(Func<IDispatchCommand,Task> callback)
        {
            return new CommandValidator(Container, () => callback(Container.Resolve<LoggingDispatcher>()));
        }
    }
}
