using Autofac;
using System;
using System.Threading;

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

        public SprayChronicleServer WithContainerConfiguration(Action<ContainerBuilder> configure)
        {
            OnConfigure += builder => configure(builder);
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
