namespace SprayChronicle.QueryHandling
{
    public interface IBuildQueryHandlers
    {
        IExecuteQueries Build<TState>(IStatefulRepository<TState> repository) where TState : class;
    }
}