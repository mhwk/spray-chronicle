namespace SprayChronicle.QueryHandling
{
    public interface IBuildProjectors
    {
        TProjector Build<TProjection,TProjector>()
            where TProjector : QueryHandler<TProjection>
            where TProjection : class;

        TProjector Build<TProjection,TProjector>(string projectionReference)
            where TProjector : QueryHandler<TProjection>
            where TProjection : class;

        TProjector Build<TProjection,TProjector>(IStatefulRepository<TProjection> repository)
            where TProjector : QueryHandler<TProjection>
            where TProjection : class;
    }
}
