using System;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoProjectorFactory : IBuildProjectors
    {
        readonly MongoRepositoryFactory _repositoryFactory;

        public MongoProjectorFactory(MongoRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public TProjector Build<TProjection,TProjector>() where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                _repositoryFactory.Build<TProjection>()
            );
        }

        public TProjector Build<TProjection,TProjector>(string projectionReference) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                _repositoryFactory.Build<TProjection>(projectionReference)
            );
        }

        public TProjector Build<TProjection,TProjector>(IStatefulRepository<TProjection> repository) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                repository
            );
        }
    }
}
