namespace SprayChronicle.QueryHandling
{
    public interface IBuildStatefulRepositories
    {
        IStatefulRepository<TState> Build<TState>() where TState : class;
        
        IStatefulRepository<TState> Build<TState>(string reference) where TState : class;
    }
}
