using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public sealed class HandlingFixture<TModule> : Fixture<ICommandDispatcher,Task,ICommandDispatcher,Task> where TModule : IModule, new()
    {
        public HandlingFixture(): this(builder => {})
        {}

        public HandlingFixture(Action<ContainerBuilder> configure)
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

        public override async Task<IExecute<ICommandDispatcher, Task>> Given(Func<ICommandDispatcher, Task> callback)
        {
            await callback(Container.Resolve<ICommandDispatcher>());
            
            return this;
        }

        public override async Task<IValidate> When(Func<ICommandDispatcher,Task> callback)
        {
            return await HandlingValidator.Run(Container, () => callback(Container.Resolve<ICommandDispatcher>()));
        }
    }
}
