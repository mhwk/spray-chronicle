using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Autofac;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Projecting
{
    public sealed class ProjectingModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register<ProjectorHandlerFactory>(
                    c => new ProjectorHandlerFactory(
                        c.Resolve<ILoggerFactory>(),
                        c.Resolve<IBuildProjectors>()
                    )
                )
                .AsSelf()
                .As<IBuildProjectorHandlers>()
                .SingleInstance();
        }

        public sealed class ProjectionWithQuery<TProjection,TProjector,TExecutor> : Autofac.Module where TProjector : Projector<TProjection> where TExecutor : OverloadQueryExecutor<TProjection>
        {
            readonly string _reference;

            readonly string _stream;

            readonly ILocateTypes _typeLocator;

            public ProjectionWithQuery(string stream, string messageNamespace)
                : this(typeof(TProjection).Name, stream, new NamespaceTypeLocator(messageNamespace))
            {}

            public ProjectionWithQuery(string reference, string stream, string messageNamespace)
                : this(reference, stream, new NamespaceTypeLocator(messageNamespace))
            {}

            public ProjectionWithQuery(string reference, string stream, ILocateTypes typeLocator)
            {
                _reference = reference;
                _stream = stream;
                _typeLocator = typeLocator;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .Register<IStatefulRepository<TProjection>>(
                        c => c.Resolve<IBuildStatefulRepositories>().Build<TProjection>(_reference)
                    )
                    .SingleInstance();
                
                builder
                    .Register<StreamEventHandler<TProjector>>(
                        c => c.Resolve<IBuildProjectorHandlers>().Build<TProjection,TProjector>(
                            c.Resolve<IBuildStreams>().CatchUp(_stream, _typeLocator),
                            c.Resolve<IStatefulRepository<TProjection>>()
                        )
                    )
                    .As<IHandleStream>()
                    .AsSelf()
                    .SingleInstance();
                
                builder
                    .RegisterType<TExecutor>()
                    .As<IExecuteQueries>()
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}
