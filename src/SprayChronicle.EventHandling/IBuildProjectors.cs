namespace SprayChronicle.EventHandling
{
    public interface IBuildProjectors
    {
        TProjector Build<TProjection,TProjector>(IStream stream) where TProjector : Projector<TProjection>;

        TProjector Build<TProjection,TProjector>(IStream stream, string projectionReference) where TProjector : Projector<TProjection>;
    }
}
