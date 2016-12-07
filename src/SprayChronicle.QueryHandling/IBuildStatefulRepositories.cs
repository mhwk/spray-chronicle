namespace SprayChronicle.QueryHandling
{
    public interface IBuildStatefulRepositories
    {
        IStatefulRepository<T> Build<T>();
        
        IStatefulRepository<T> Build<T>(string reference);
    }
}
