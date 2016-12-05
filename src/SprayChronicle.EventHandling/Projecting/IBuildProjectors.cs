namespace SprayChronicle.EventHandling.Projecting
{
    public interface IBuildProjectors
    {
        TProjector Build<TProjection,TProjector>() where TProjector : Projector<TProjection>;

        TProjector Build<TProjection,TProjector>(string projectionReference) where TProjector : Projector<TProjection>;

        TProjector Build<TProjection,TProjector>(IProjectionRepository<TProjection> repository) where TProjector : Projector<TProjection>;
    }
}
