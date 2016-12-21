using System;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Testing
{
    public class ProjectionQueryFixture<TModule> : IPopulate, IExecute where TModule : IModule, new()
    {
        readonly IContainer _container;

        public ProjectionQueryFixture(Action<ContainerBuilder> configure)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<SprayChronicle.EventHandling.SyncEventHandlingModule>();
            builder.RegisterModule<SprayChronicle.Persistence.Memory.MemoryModule>();
            builder.RegisterModule<SprayChronicle.Projecting.ProjectingModule>();
            builder.RegisterModule<SprayChronicle.QueryHandling.QueryHandlingModule>();
            builder.RegisterModule<TModule>();
            builder.Register<ILoggerFactory>(c => new LoggerFactory().AddConsole(LogLevel.Debug).AddDebug(LogLevel.Debug)).SingleInstance();
            builder.Register<TestStream>(c => new TestStream()).SingleInstance();
            builder.Register<IBuildStreams>(c => new TestStreamFactory(c.Resolve<TestStream>())).SingleInstance();
            configure(builder);
            _container = builder.Build();
            _container.Resolve<IManageStreamHandlers>().Manage();
        }

        public IExecute Given(params object[] messages)
        {
            foreach (var message in messages) {
                _container.Resolve<TestStream>().Publish(message, new DateTime());
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
