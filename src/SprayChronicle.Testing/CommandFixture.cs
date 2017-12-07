using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Testing
{
    public sealed class CommandFixture<TModule> : Fixture<TModule,IDispatchCommands,Task,IDispatchCommands,Task> where TModule : IModule, new()
    {
        public CommandFixture(): this(builder => {})
        {}

        public CommandFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<CommandHandlingModule>();
                builder.RegisterModule<SyncEventHandlingModule>();
                builder.RegisterModule<MemoryModule>();
                builder.RegisterModule<QueryHandlingModule>();
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
        
        protected override void Boot()
        {
            Container.Resolve<IManageStreamHandlers>().Manage();
        }

        public override async Task<IExecute<IDispatchCommands, Task>> Given(Func<IDispatchCommands, Task> callback)
        {
            await callback(Container.Resolve<ErrorSuppressingDispatcher>());
            
            return this;
        }

        public override async Task<IValidate> When(Func<IDispatchCommands,Task> callback)
        {
            return await CommandValidator.Run(Container, () => callback(Container.Resolve<LoggingDispatcher>()));
        }
    }
}
