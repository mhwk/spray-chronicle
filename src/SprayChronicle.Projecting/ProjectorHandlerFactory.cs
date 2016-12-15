using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Projecting
{
    public class ProjectorHandlerFactory : IBuildProjectorHandlers
    {
        readonly ILoggerFactory _loggerFactory;

        readonly IBuildProjectors _projectorFactory;

        public ProjectorHandlerFactory(
            ILoggerFactory loggerFactory,
            IBuildProjectors projectorFactory)
        {
            _loggerFactory = loggerFactory;
            _projectorFactory = projectorFactory;
        }

        public StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream) where TProjector : Projector<TProjection>
        {
            return new StreamEventHandler<TProjector>(
                _loggerFactory.CreateLogger<TProjector>(),
                stream,
                _projectorFactory.Build<TProjection,TProjector>()
            );
        }

        public StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream, string projectionReference) where TProjector : Projector<TProjection>
        {
            return new StreamEventHandler<TProjector>(
                _loggerFactory.CreateLogger<TProjector>(),
                stream,
                _projectorFactory.Build<TProjection,TProjector>(projectionReference)
            );
        }

        public StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream, IStatefulRepository<TProjection> repository) where TProjector : Projector<TProjection>
        {
            return new StreamEventHandler<TProjector>(
                _loggerFactory.CreateLogger<TProjector>(),
                stream,
                _projectorFactory.Build<TProjection,TProjector>(repository)
            );
        }
    }
}