namespace SprayChronicle.QueryHandling
{
    public sealed class OverloadQueryExecutorFactory : IBuildQueryExecutors
    {
        public IExecuteQueries Build<TState>(IStatefulRepository<TState> repository)
        {
            return new OverloadQueryExecutor<TState>(repository);
        }
    }
}
