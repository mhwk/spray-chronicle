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

        static ILoggerFactory _loggerFactory = new LoggerFactory().AddConsole(LogLevel.Information);

        static IContainer _container;

        public delegate void ConfigurationHandler(ContainerBuilder builder);

        public event ConfigurationHandler OnConfigure;

        public delegate void InitializationHandler();

        public event InitializationHandler OnInitialize;

        public delegate void ExecutionHandler(IContainer server);

        public event ExecutionHandler OnExecute;

        public SprayChronicleServer WithLogLevel(LogLevel logLevel)
        {
            _loggerFactory.AddConsole(logLevel);
            return this;
        }

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

        public static ILoggerFactory LoggerFactory()
        {
            return _loggerFactory;
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
            if (null != OnConfigure) {
                OnConfigure(ContainerBuilder());
            }

            ContainerBuilder().Register<ILoggerFactory>(c => LoggerFactory());

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
