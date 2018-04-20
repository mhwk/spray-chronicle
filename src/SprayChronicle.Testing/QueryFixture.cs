using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Testing
{
    public class QueryFixture<TModule,TTarget> : Fixture<TestSource<TTarget>,Task,QueryRouter,Task<object>>
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
            // @todo cancellation token
            Container.Resolve<IPipelineManager>().Start();
        }

        public override async Task<IExecute<QueryRouter, Task<object>>> Given(Func<TestSource<TTarget>, Task> callback)
        {
            var stream = Container.Resolve<TestSource<TTarget>>();
            await callback(stream);
            return this;
        }

        public override async Task<IValidate> When(Func<QueryRouter, Task<object>> callback)
        {
            return await QueryValidator.Run(Container, () => callback(Container.Resolve<QueryRouter>()));
        }
    }
}
