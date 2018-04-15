using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Persistence.Memory;

namespace SprayChronicle.Testing
{
    public sealed class CommandFixture<TModule> : Fixture<TModule,ICommandRouter,Task,ICommandRouter,Task> where TModule : IModule, new()
    {
        public CommandFixture(): this(builder => {})
        {}

        public CommandFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<CommandHandlingModule>();
                builder.RegisterModule<SyncEventHandlingModule>();
                builder.RegisterModule<MemoryModule>();
                builder
                    .Register(c => new TestStore(
                        c.Resolve<MemoryEventStore>(),
                        c.Resolve<EpochGenerator>()
                    ))
                    .AsSelf()
                    .As<IEventStore>()
                    .SingleInstance();
                builder.RegisterModule<TModule>();
                configure(builder);
            })
        {
        }
        
        protected override void Boot()
        {
            Container.Resolve<IManageStreamHandlers>().Manage();
        }

        public override async Task<IExecute<ICommandRouter, Task>> Given(Func<ICommandRouter, Task> callback)
        {
            await callback(Container.Resolve<ErrorSuppressingRouter>());
            
            return this;
        }

        public override async Task<IValidate> When(Func<ICommandRouter,Task> callback)
        {
            return await CommandValidator.Run(Container, () => callback(Container.Resolve<LoggingRouter>()));
        }
    }
}
