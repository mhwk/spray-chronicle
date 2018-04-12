using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using SprayChronicle.Server;

namespace SprayChronicle.EventHandling
{
    public abstract class EventHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => CreateManager())
                .OnActivating(e => RegisterStreamHandlers(e.Context, e.Instance))
                .SingleInstance();
        }

        protected abstract IManageStreamHandlers CreateManager();

        private static void RegisterStreamHandlers(IComponentContext context, IManageStreamHandlers manager)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleStream>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleStream)
                .ToList()
                .ForEach(manager.Add);
        }

        public sealed class CatchUp<THandler> : Module where THandler : IProcessEvents
        {
            private readonly string _stream;

            public CatchUp(string stream)
            {
                _stream = stream;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .RegisterType<THandler>()
                    .SingleInstance();
                
//                builder
//                    .Register(c => new StreamHandler<THandler>(
//                        c.Resolve<ILoggerFactory>().Create<THandler>(),
//                        new MeasureMilliseconds(),
//                        c.Resolve<IBuildStreams>().CatchUp(_stream),
//                        c.Resolve<THandler>()
//                    ))
//                    .As<IHandleStream>()
//                    .AsSelf()
//                    .SingleInstance();
            }
        }

        public sealed class Persistent<THandler> : Module where THandler : IProcessEvents
        {
            private readonly string _stream;

            private readonly string _category;

            private readonly Func<IComponentRegistry,bool> _condition;

            public Persistent(string stream, string category, Func<IComponentRegistry,bool> condition)
            {
                _stream = stream;
                _category = category;
                _condition = condition;
            }

            public Persistent(string stream, string category)
                : this(stream, category, reg => true)
            {}

            public Persistent(string stream)
                : this(stream, typeof(THandler).FullName)
            {}

            public Persistent(string stream, Func<IComponentRegistry,bool> condition)
                : this(stream, typeof(THandler).FullName, condition)
            {}

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .RegisterType<THandler>()
                    .OnlyIf(reg => _condition(reg) && ! reg.IsRegistered(new TypedService(typeof(THandler))))
                    .SingleInstance();
                    
//                builder
//                    .Register(c => new StreamHandler<THandler>(
//                        c.Resolve<ILoggerFactory>().Create<THandler>(),
//                        new MeasureMilliseconds(),
//                        c.Resolve<IBuildStreams>().Persistent(
//                            _stream,
//                            _category
//                        ),
//                        c.Resolve<THandler>()
//                    ))
//                    .As<IHandleStream>()
//                    .AsSelf()
//                    .OnlyIf(reg => _condition(reg))
//                    .SingleInstance();
            }
        }
    }
}
