using Microsoft.Extensions.Logging;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Projecting
{
    public class ProjectorHandlerFactory : IBuildProjectorHandlers
    {
        readonly ILogger<IStream> _logger;

        readonly IBuildProjectors _projectorFactory;

        public ProjectorHandlerFactory(
            ILogger<IStream> logger,
            IBuildProjectors projectorFactory)
        {
            _logger = logger;
            _projectorFactory = projectorFactory;
        }

        public StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream) where TProjector : Projector<TProjection>
        {
            return new StreamEventHandler<TProjector>(
                _logger,
                stream,
                _projectorFactory.Build<TProjection,TProjector>()
            );
        }

        public StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream, string projectionReference) where TProjector : Projector<TProjection>
        {
            return new StreamEventHandler<TProjector>(
                _logger,
                stream,
                _projectorFactory.Build<TProjection,TProjector>(projectionReference)
            );
        }

        public StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream, IStatefulRepository<TProjection> repository) where TProjector : Projector<TProjection>
        {
            return new StreamEventHandler<TProjector>(
                _logger,
                stream,
                _projectorFactory.Build<TProjection,TProjector>(repository)
            );
        }
    }
}