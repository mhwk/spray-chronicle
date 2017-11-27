using System.Linq;
using Microsoft.Extensions.Logging;
using Autofac;
using SprayChronicle.Server;

namespace SprayChronicle.EventHandling
{
    public abstract class EventHandlingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<IManageStreamHandlers>(c => CreateManager())
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

        public sealed class CatchUp<THandler> : Autofac.Module where THandler : IHandleEvent
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
                
                builder
                    .Register(c => new StreamEventHandler<THandler>(
                        c.Resolve<ILoggerFactory>().CreateLogger<THandler>(),
                        c.Resolve<IBuildStreams>().CatchUp(_stream),
                        c.Resolve<THandler>()
                    ))
                    .As<IHandleStream>()
                    .AsSelf()
                    .SingleInstance();
            }
        }

        public sealed class Persistent<THandler> : Module where THandler : IHandleEvent
        {
            private readonly string _stream;

            private readonly string _category;

            public Persistent(string stream, string category)
            {
                _stream = stream;
                _category = category;
            }

            public Persistent(string stream)
                : this(stream, typeof(THandler).FullName)
            {}

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .RegisterType<THandler>()
                    .SingleInstance();
                    
                builder
                    .Register(c => new StreamEventHandler<THandler>(
                        c.Resolve<ILoggerFactory>().CreateLogger<THandler>(),
                        c.Resolve<IBuildStreams>().Persistent(
                            _stream,
                            _category
                        ),
                        c.Resolve<THandler>()
                    ))
                    .As<IHandleStream>()
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}
