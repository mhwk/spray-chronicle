using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Testing
{
    public class EventSourcedFixture<TModule,TSourced> : Fixture,
        IPopulate<Task<TSourced>,TSourced,Task<TSourced>>,
        IExecute<TSourced,Task<TSourced>>
        where TModule : IModule, new()
        where TSourced : EventSourced<TSourced>
    {
        private TSourced _sourced;

        public EventSourcedFixture(): this(builder => {})
        {}

        public EventSourcedFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<TModule>();
                configure(builder);
            })
        {
        }

        public async Task<IExecute<TSourced,Task<TSourced>>> Given(Func<Task<TSourced>> callback)
        {
            _sourced = await callback();
            _sourced?.Diff();
            
            return this;
        }

        public async Task<IValidate> When(Func<TSourced,Task<TSourced>> callback)
        {
            return await EventSourcedValidator.Run(Container, () => callback(_sourced));
        }
    }
}
