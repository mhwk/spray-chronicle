using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.Logging;
using Autofac;

namespace SprayChronicle.EventHandling
{
    public sealed class EventHandlingModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<IManageStreamHandlers>(c => new StreamHandlerManager())
                .OnActivating(e => RegisterStreamHandlers(e.Context, e.Instance as IManageStreamHandlers))
                .SingleInstance();
        }

        void RegisterStreamHandlers(IComponentContext context, IManageStreamHandlers manager)
        {
            context.ComponentRegistry.Registrations
                .Where(r => r.Activator.LimitType.IsAssignableTo<IHandleStream>())
                .Select(r => context.Resolve(r.Activator.LimitType) as IHandleStream)
                .ToList()
                .ForEach(h => manager.Add(h));
        }

        static object[] BuildArguments<T>(IComponentContext context)
        {
            var args = new List<object>();

            var constructor = typeof(T).GetTypeInfo().GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();
            
            if (null == constructor) {
                return args.ToArray();
            }

            var types = constructor.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
            
            for (var i = 0; i < types.Length; i++) {
                args.Add(context.Resolve(types[i]));
            }

            return args.ToArray();
        }

        public sealed class CatchUp<THandler> : Autofac.Module where THandler : IHandleEvent
        {
            readonly string _stream;

            readonly string _namespace;

            public CatchUp(string stream, string @namespace)
            {
                _stream = stream;
                _namespace = @namespace;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .Register<StreamEventHandler<THandler>>(c => new StreamEventHandler<THandler>(
                        c.Resolve<ILoggerFactory>().CreateLogger<THandler>(),
                        c.Resolve<IBuildStreams>().CatchUp(
                            _stream,
                            new NamespaceTypeLocator(_namespace)
                        ),
                        Activator.CreateInstance(typeof(THandler), BuildArguments<THandler>(c)) as IHandleEvent
                    ))
                    .As<IHandleStream>()
                    .AsSelf()
                    .InstancePerDependency();
            }
        }

        public sealed class Persistent<THandler> : Autofac.Module where THandler : IHandleEvent
        {
            readonly string _stream;

            readonly string _category;

            readonly string _namespace;

            public Persistent(string stream, string category, string @namespace)
            {
                _stream = stream;
                _category = category;
                _namespace = @namespace;
            }

            public Persistent(string stream, string @namespace)
                : this(stream, typeof(THandler).FullName, @namespace)
            {}

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .Register<StreamEventHandler<THandler>>(c => new StreamEventHandler<THandler>(
                        c.Resolve<ILoggerFactory>().CreateLogger<THandler>(),
                        c.Resolve<IBuildStreams>().Persistent(
                            _stream,
                            _category,
                            new NamespaceTypeLocator(_namespace)
                        ),
                        Activator.CreateInstance(typeof(THandler), BuildArguments<THandler>(c)) as IHandleEvent
                    ))
                    .As<IHandleStream>()
                    .AsSelf()
                    .InstancePerDependency();
            }
        }
    }
}
