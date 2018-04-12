namespace SprayChronicle.QueryHandling
{
    public interface IBuildProjectors
    {
        TProjector Build<TProjection,TProjector>()
            where TProjection : class;

        TProjector Build<TProjection,TProjector>(string projectionReference)
            where TProjection : class;

        TProjector Build<TProjection,TProjector>(IStatefulRepository<TProjection> repository)
            where TProjection : class;
    }
}
