using System;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Raven
{
    public class RavenDbProjectorFactory : IBuildProjectors
    {
        private readonly RavenDbRepositoryFactory _repositoryFactory;

        public RavenDbProjectorFactory(RavenDbRepositoryFactory repositoryFactory)
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
