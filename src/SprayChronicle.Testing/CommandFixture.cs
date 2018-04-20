using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public sealed class CommandFixture<TModule> : Fixture<CommandRouter,Task,CommandRouter,Task> where TModule : IModule, new()
    {
        public CommandFixture(): this(builder => {})
        {}

        public CommandFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<CommandHandlingModule>();
                builder.RegisterModule<EventHandlingModule>();
                builder
                    .Register(c => new TestStore(
                        c.Resolve<IEventStore>(),
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
            // @todo cancellationtoken
            
            Container.Resolve<IPipelineManager>().Start();
        }

        public override async Task<IExecute<CommandRouter, Task>> Given(Func<CommandRouter, Task> callback)
        {
            await callback(Container.Resolve<CommandRouter>());
            
            return this;
        }

        public override async Task<IValidate> When(Func<CommandRouter,Task> callback)
        {
            return await CommandValidator.Run(Container, () => callback(Container.Resolve<CommandRouter>()));
        }
    }
}
