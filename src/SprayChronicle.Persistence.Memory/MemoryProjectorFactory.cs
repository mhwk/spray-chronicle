using System;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryProjectorFactory : IBuildProjectors
    {
        private readonly IBuildStatefulRepositories _repositoryFactory;

        public MemoryProjectorFactory(
            ILoggerFactory loggerFactory,
            IBuildStatefulRepositories repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public TProjector Build<TProjection,TProjector>()
            where TProjector : QueryHandler<TProjection>
            where TProjection : class
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                _repositoryFactory.Build<TProjection>()
            );
        }

        public TProjector Build<TProjection,TProjector>(string projectionReference)
            where TProjector : QueryHandler<TProjection>
            where TProjection : class
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                _repositoryFactory.Build<TProjection>(projectionReference)
            );
        }

        public TProjector Build<TProjection,TProjector>(IStatefulRepository<TProjection> repository)
            where TProjector : QueryHandler<TProjection>
            where TProjection : class
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                repository
            );
        }
    }
}