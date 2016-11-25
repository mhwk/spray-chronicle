namespace SprayChronicle.EventHandling.Projecting
{
    public interface IBuildProjectionRepositories
    {
        IProjectionRepository<T> Build<T>();
        
        IProjectionRepository<T> Build<T>(string reference);
    }
}
