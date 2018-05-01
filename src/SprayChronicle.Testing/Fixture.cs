using System;
using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.Server;

namespace SprayChronicle.Testing
{
    public abstract class Fixture
    {
        protected readonly IContainer Container;

        protected Fixture(): this(builder => {})
        {}

        protected Fixture(Action<ContainerBuilder> configure)
        {
            var builder = new ContainerBuilder();

            builder
                .Register(c => new LoggerFactory().AddConsole(LogLevel.Debug))
                .SingleInstance();
            
            builder.Register(c => new EpochGenerator()).SingleInstance();
            builder.RegisterModule<ChronicleServerModule>();
            configure(builder);

            Container = builder.Build();
            Boot();
        }

        protected virtual void Boot()
        {
        }
    }
}
