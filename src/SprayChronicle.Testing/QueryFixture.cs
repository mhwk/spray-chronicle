using System;
using System.Collections.Generic;
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
    public class QueryFixture<TModule> : Fixture<TModule,IProcessQueries,Task<object>>, IPopulateEpoch<IProcessQueries,Task<object>>
        where TModule : IModule, new()
    {
        private readonly List<DateTime> _epochs = new List<DateTime>();

        public QueryFixture(Action<ContainerBuilder> configure)
            : base(builder => {
                builder.RegisterModule<CommandHandlingModule>();
                builder.RegisterModule<SyncEventHandlingModule>();
                builder.RegisterModule<MemoryModule>();
                builder.RegisterModule<ProjectingModule>();
                builder.RegisterModule<QueryHandlingModule>();
                builder.Register<ILoggerFactory>(c => new LoggerFactory().AddConsole(LogLevel)).SingleInstance();
                builder.Register<TestStream>(c => new TestStream()).SingleInstance();
                builder.Register<IBuildStreams>(c => new TestStreamFactory(c.Resolve<TestStream>())).SingleInstance();
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

        public IPopulate<IProcessQueries,Task<object>> Epoch(params DateTime[] epochs)
        {
            _epochs.AddRange(epochs);
            return this;
        }

        public override IExecute<IProcessQueries,Task<object>> Given(params object[] messages)
        {
            for (var i = 0; i < messages.Length; i++) {
                if (_epochs.Count > i) {
                    Container.Resolve<TestStream>().Publish(messages[i].ToMessage(), _epochs[i]);
                } else {
                    Container.Resolve<TestStream>().Publish(messages[i].ToMessage(), DateTime.Now);
                }
            }
            return this;
        }

        public override async Task<IValidate> When(Func<IProcessQueries,Task<object>> callback)
        {
            return await QueryValidator.Run(Container, () => callback(Container.Resolve<LoggingQueryProcessor>()));
        }
    }
}
