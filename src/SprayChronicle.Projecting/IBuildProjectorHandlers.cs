using SprayChronicle.EventHandling;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Projecting
{
    public interface IBuildProjectorHandlers
    {
        StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream) where TProjector : Projector<TProjection>;

        StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream, string projectionReference) where TProjector : Projector<TProjection>;

        StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream, IStatefulRepository<TProjection> repository) where TProjector : Projector<TProjection>;
    }
}