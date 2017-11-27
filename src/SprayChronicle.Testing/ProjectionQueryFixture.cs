using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Testing
{
    public class ProjectionQueryFixture<TModule> : IPopulateEpoch, IPopulate, IExecute where TModule : IModule, new()
    {
        private readonly IContainer _container;

        private List<DateTime> _epochs = new List<DateTime>();

        protected const LogLevel LogLevel = Microsoft.Extensions.Logging.LogLevel.Information;

        public ProjectionQueryFixture(Action<ContainerBuilder> configure)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<SprayChronicle.CommandHandling.CommandHandlingModule>();
            builder.RegisterModule<SprayChronicle.EventHandling.SyncEventHandlingModule>();
            builder.RegisterModule<SprayChronicle.Persistence.Memory.MemoryModule>();
            builder.RegisterModule<SprayChronicle.Projecting.ProjectingModule>();
            builder.RegisterModule<SprayChronicle.QueryHandling.QueryHandlingModule>();
            builder.RegisterModule<TModule>();
            builder.Register<ILoggerFactory>(c => new LoggerFactory().AddConsole(LogLevel)).SingleInstance();
            builder.Register<TestStream>(c => new TestStream()).SingleInstance();
            builder.Register<IBuildStreams>(c => new TestStreamFactory(c.Resolve<TestStream>())).SingleInstance();
            configure(builder);
            _container = builder.Build();
            _container.Resolve<IManageStreamHandlers>().Manage();
        }

        public IPopulate Epoch(params DateTime[] epochs)
        {
            _epochs.AddRange(epochs);
            return this;
        }

        public IExecute Given(params object[] messages)
        {
            for (var i = 0; i < messages.Length; i++) {
                if (_epochs.Count > i) {
                    _container.Resolve<TestStream>().Publish(new InstanceMessage(messages[i]), _epochs[i]);
                } else {
                    _container.Resolve<TestStream>().Publish(new InstanceMessage(messages[i]), DateTime.Now);
                }
            }
            return this;
        }

        public IValidate When(object query)
        {
            try {
                return new ProjectionValidator(_container.Resolve<LoggingQueryProcessor>().Process(query));
            } catch (Exception error) {
                return new ProjectionValidator(error);
            }
        }
    }
}
