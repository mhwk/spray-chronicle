namespace SprayChronicle.EventHandling.Projecting
{
    public interface IBuildProjectorHandlers
    {
        StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream) where TProjector : Projector<TProjection>;

        StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream, string projectionReference) where TProjector : Projector<TProjection>;

        StreamEventHandler<TProjector> Build<TProjection,TProjector>(IStream stream, IProjectionRepository<TProjection> repository) where TProjector : Projector<TProjection>;
    }
}