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
            private readonly string _reference;

            private readonly string _stream;

            public ProjectionWithQuery(string stream)
                : this(typeof(TProjection).Name, stream)
            {}

            public ProjectionWithQuery(string reference, string stream)
            {
                _reference = reference;
                _stream = stream;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder
                    .Register<IStatefulRepository<TProjection>>(
                        c => c.Resolve<IBuildStatefulRepositories>().Build<TProjection>(_reference)
                    )
                    .SingleInstance();
                
                builder
                    .RegisterType<TProjector>()
                    .AsSelf()
                    .SingleInstance();

                builder
                    .Register<StreamEventHandler<TProjector>>(
                        c => new StreamEventHandler<TProjector>(
                            c.Resolve<ILoggerFactory>().CreateLogger<TProjector>(),
                            c.Resolve<IBuildStreams>().CatchUp(_stream),
                            c.Resolve<TProjector>()
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
