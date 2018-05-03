﻿using Autofac;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;
using SprayChronicle.Testing;

namespace SprayChronicle.Persistence.Ouro.Test
{
    public abstract class OuroTestCase
    {
        private IContainer _container;
        
        public IContainer Container()
        {
            if (null == _container) {
                var builder = new ContainerBuilder();
                builder.RegisterModule<OuroModule>();
                builder.Register(c => new ConsoleLoggerFactory()).As<ILoggerFactory>().SingleInstance();
                builder.Register(c => new TestSourceFactory(c.Resolve<ILoggerFactory>())).As<IEventSourceFactory>().SingleInstance();

                Configure(builder);

                _container = builder.Build();
            }

            return _container;
        }

        protected abstract void Configure(ContainerBuilder builder);
    }
}