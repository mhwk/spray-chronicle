using System;

namespace SprayChronicle.EventHandling.MongoDB
{
    public sealed class MongoProjectorFactory : IBuildProjectors
    {
        readonly MongoRepositoryFactory _repositoryFactory;

        public MongoProjectorFactory(MongoRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public TProjector Build<TProjection,TProjector>(IStream stream) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                stream,
                new BufferedRepository<TProjection>(
                    _repositoryFactory.Build<TProjection>()
                )
            );
        }

        public TProjector Build<TProjection,TProjector>(IStream stream, string projectionReference) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                stream,
                new BufferedRepository<TProjection>(
                    _repositoryFactory.Build<TProjection>(projectionReference)
                )
            );
        }
    }
}
