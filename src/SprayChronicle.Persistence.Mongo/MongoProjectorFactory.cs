using System;
using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Mongo
{
    public sealed class MongoProjectorFactory : IBuildProjectors
    {
        readonly ILoggerFactory _loggerFactory;

        readonly MongoRepositoryFactory _repositoryFactory;

        public MongoProjectorFactory(ILoggerFactory loggerFactory, MongoRepositoryFactory repositoryFactory)
        {
            _loggerFactory = loggerFactory;
            _repositoryFactory = repositoryFactory;
        }

        public TProjector Build<TProjection,TProjector>() where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedStateRepository<TProjection>(
                    _loggerFactory.CreateLogger<TProjection>(),
                    _repositoryFactory.Build<TProjection>()
                )
            );
        }

        public TProjector Build<TProjection,TProjector>(string projectionReference) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedStateRepository<TProjection>(
                    _loggerFactory.CreateLogger<TProjection>(),
                    _repositoryFactory.Build<TProjection>(projectionReference)
                )
            );
        }

        public TProjector Build<TProjection,TProjector>(IStatefulRepository<TProjection> repository) where TProjector : Projector<TProjection>
        {
            return (TProjector) Activator.CreateInstance(
                typeof(TProjector),
                new BufferedStateRepository<TProjection>(
                    _loggerFactory.CreateLogger<TProjection>(),
                    repository
                )
            );
        }
    }
}
