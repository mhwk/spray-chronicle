using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using SprayChronicle.Server;

namespace SprayChronicle.Testing
{
    public abstract class Fixture<TModule,TPopulate,TPopulateResult,TExecute,TValidate> : IPopulate<TPopulate,TPopulateResult,TExecute,TValidate>, IExecute<TExecute,TValidate>
        where TModule : IModule, new()
        where TPopulate : class
        where TPopulateResult : class
        where TExecute : class
        where TValidate : class
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

        public abstract Task<IExecute<TExecute, TValidate>> Given(Func<TPopulate, TPopulateResult> callback);

        public abstract Task<IValidate> When(Func<TExecute, TValidate> callback);
    }
}
