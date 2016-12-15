using Autofac;
using Autofac.Core;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace SprayChronicle.Server
{
    public class SprayChronicleServer
    {
        static ContainerBuilder _containerBuilder = new ContainerBuilder();

        static IContainer _container;

        public delegate void ConfigurationHandler(ContainerBuilder builder);

        public event ConfigurationHandler OnConfigure;

        public delegate void InitializationHandler();

        public event InitializationHandler OnInitialize;

        public delegate void ExecutionHandler(IContainer server);

        public event ExecutionHandler OnExecute;

        public SprayChronicleServer WithAutofacModule(Module module)
        {
            OnConfigure += builder => builder.RegisterModule(module);
            return this;
        }

        public SprayChronicleServer WithAutofacModule<T>() where T : IModule, new()
        {
            OnConfigure += builder => builder.RegisterModule<T>();
            return this;
        }

        public static ContainerBuilder ContainerBuilder()
        {
            if (null != _container) {
                throw new Exception("Container already built");
            }
            return _containerBuilder;
        }

        public static IContainer Container()
        {
            if (null == _container) {
                _container = _containerBuilder.Build();
            }
            return _container;
        }

        public void Run()
        {
            ContainerBuilder().Register<ILoggerFactory>(c => {
                var factory = new LoggerFactory();
                #if DEBUG
                factory.AddConsole(LogLevel.Debug);
                #else
                factory.AddConsole();
                #endif
                return factory;
            });

            if (null != OnConfigure) {
                OnConfigure(ContainerBuilder());
            }

            if (null != OnInitialize) {
                OnInitialize();
            }

            if (null != OnExecute) {
                OnExecute(Container());
            }

            new AutoResetEvent(false).WaitOne();
        }
    }
}
