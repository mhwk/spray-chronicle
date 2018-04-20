using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class EventSourcedFixture<TModule,TSourced> : Fixture<TSourced,Task<TSourced>,TSourced,Task<TSourced>>
        where TModule : IModule, new()
        where TSourced : EventSourced<TSourced>
    {
        private TSourced _sourced;

        public EventSourcedFixture(): this(builder => {})
        {}

        public EventSourcedFixture(Action<ContainerBuilder> configure)
            : base(builder => {
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

        public override async Task<IExecute<TSourced, Task<TSourced>>> Given(Func<TSourced, Task<TSourced>> callback)
        {
            _sourced = await callback(null);
            _sourced?.Diff();
            
            return this;
        }

        public override async Task<IValidate> When(Func<TSourced,Task<TSourced>> callback)
        {
            return await EventSourcedValidator.Run(Container, () => callback(_sourced));
        }
    }
}
