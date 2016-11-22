namespace SprayChronicle.EventHandling
{
    public interface IBuildProjectionRepositories
    {
        IProjectionRepository<T> Build<T>();
        
        IProjectionRepository<T> Build<T>(string reference);
    }
}
