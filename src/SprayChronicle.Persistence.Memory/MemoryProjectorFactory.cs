using System;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.EventHandling.Projecting;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryProjectorFactory : IBuildProjectors
    {
        readonly ILogger<IStream> _logger;

        readonly IBuildProjectionRepositories _repositoryFactory;

        public MemoryProjectorFactory(
            ILogger<IStream> logger,
            IBuildProjectionRepositories repositoryFactory)
        {
            _logger = logger;
            _repositoryFactory = repositoryFactory;
        }

        public TProjector Build<TProjection,TProjector>() where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedRepository<TProjection>(
                    _logger,
                    _repositoryFactory.Build<TProjection>()
                )
            );
        }

        public TProjector Build<TProjection,TProjector>(string projectionReference) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedRepository<TProjection>(
                    _logger,
                    _repositoryFactory.Build<TProjection>(projectionReference)
                )
            );
        }

        public TProjector Build<TProjection,TProjector>(IProjectionRepository<TProjection> repository) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedRepository<TProjection>(
                    _logger,
                    repository
                )
            );
        }
    }
}