using SprayChronicle.QueryHandling;

namespace SprayChronicle.Projecting
{
    public interface IBuildProjectors
    {
        TProjector Build<TProjection,TProjector>() where TProjector : Projector<TProjection>;

        TProjector Build<TProjection,TProjector>(string projectionReference) where TProjector : Projector<TProjection>;

        TProjector Build<TProjection,TProjector>(IStatefulRepository<TProjection> repository) where TProjector : Projector<TProjection>;
    }
}
