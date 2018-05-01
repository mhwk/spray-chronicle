using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Testing
{
    public class QueryFixture<TModule,TTarget> : Fixture,
        IPopulate<TestSource<TTarget>,Task,IQueryDispatcher,Task<object>>,
        IExecute<IQueryDispatcher,Task<object>>
        where TModule : IModule, new()
        where TTarget : class
    {
        public QueryFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<EventHandlingModule>();
                builder.RegisterModule<QueryHandlingModule>();
                builder.RegisterModule<TModule>();
                configure(builder);
            })
        {
        }
        
        public QueryFixture(): this(builder => { })
        {
        }

        protected override void Boot()
        {
            Container.Resolve<IPipelineManager>().Start();
        }

        public async Task<IExecute<IQueryDispatcher, Task<object>>> Given(Func<TestSource<TTarget>, Task> callback)
        {
            var source = Container.Resolve<TestSource<TTarget>>();
            source.Complete();
            await callback(source);
            return this;
        }

        public async Task<IValidate> When(Func<IQueryDispatcher, Task<object>> callback)
        {
            return await QueryValidator.Run(Container, () => callback(Container.Resolve<IQueryDispatcher>()));
        }
    }
}
