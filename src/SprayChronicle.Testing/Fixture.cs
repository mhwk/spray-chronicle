using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;

namespace SprayChronicle.Testing
{
    public abstract class Fixture<TModule,TExecute,TValidate> : IPopulate<TExecute,TValidate>, IExecute<TExecute,TValidate>
        where TModule : IModule, new()
        where TExecute : class
        where TValidate : class
    {
        public static LogLevel LogLevel = Microsoft.Extensions.Logging.LogLevel.Information;
        
        protected readonly IContainer Container;

        protected Fixture(): this(builder => {})
        {}

        protected Fixture(Action<ContainerBuilder> configure)
        {
            var builder = new ContainerBuilder();
            
            configure(builder);
            builder.RegisterModule<TModule>();
            builder.Register<ILoggerFactory>(c => new LoggerFactory().AddConsole(LogLevel));

            Container = builder.Build();
            Boot();
        }

        protected virtual void Boot()
        {
        }

        public abstract IExecute<TExecute,TValidate> Given(params object[] messages);

        public abstract Task<IValidate> When(Func<TExecute, TValidate> callback);
    }
}
