using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Memory;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Testing
{
    public class QueryFixture<TModule> : Fixture<TModule,TestStream,Task,IProcessQueries,Task<object>>
        where TModule : IModule, new()
    {
        public QueryFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<SyncEventHandlingModule>();
                builder.RegisterModule<MemoryModule>();
                builder.RegisterModule<QueryHandlingModule>();
                builder.Register(c => new TestStream(c.Resolve<EpochGenerator>())).SingleInstance();
                builder.Register<IBuildStreams>(c => new TestStreamFactory(c.Resolve<TestStream>())).SingleInstance();
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
            Container.Resolve<IManageStreamHandlers>().Manage();
        }

        public override async Task<IExecute<IProcessQueries, Task<object>>> Given(Func<TestStream, Task> callback)
        {
            var stream = Container.Resolve<TestStream>();
            await callback(stream);
            return this;
        }

        public override async Task<IValidate> When(Func<IProcessQueries, Task<object>> callback)
        {
            return await QueryValidator.Run(Container, () => callback(Container.Resolve<LoggingProcessor>()));
        }
    }
}
