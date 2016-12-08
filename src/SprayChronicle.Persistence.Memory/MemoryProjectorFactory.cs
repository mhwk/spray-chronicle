using System;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryProjectorFactory : IBuildProjectors
    {
        readonly ILoggerFactory _loggerFactory;

        readonly IBuildStatefulRepositories _repositoryFactory;

        public MemoryProjectorFactory(
            ILoggerFactory loggerFactory,
            IBuildStatefulRepositories repositoryFactory)
        {
            _loggerFactory = loggerFactory;
            _repositoryFactory = repositoryFactory;
        }

        public TProjector Build<TProjection,TProjector>() where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedStateRepository<TProjection>(
                    _loggerFactory.AddConsole(LogLevel.Debug).CreateLogger<TProjection>(),
                    _repositoryFactory.Build<TProjection>()
                )
            );
        }

        public TProjector Build<TProjection,TProjector>(string projectionReference) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedStateRepository<TProjection>(
                    _loggerFactory.AddConsole(LogLevel.Debug).CreateLogger<TProjection>(),
                    _repositoryFactory.Build<TProjection>(projectionReference)
                )
            );
        }

        public TProjector Build<TProjection,TProjector>(IStatefulRepository<TProjection> repository) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedStateRepository<TProjection>(
                    _loggerFactory.AddConsole(LogLevel.Debug).CreateLogger<TProjection>(),
                    repository
                )
            );
        }
    }
}