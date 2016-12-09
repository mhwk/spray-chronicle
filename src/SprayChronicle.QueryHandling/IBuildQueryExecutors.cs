namespace SprayChronicle.QueryHandling
{
    public interface IBuildQueryExecutors
    {
        IExecuteQueries Build<TState>(IStatefulRepository<TState> repository);
    }
}