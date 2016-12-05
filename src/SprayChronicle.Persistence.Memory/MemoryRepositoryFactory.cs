using SprayChronicle.EventHandling.Projecting;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryRepositoryFactory : IBuildProjectionRepositories
    {
        public IProjectionRepository<T> Build<T>()
        {
            return new MemoryRepository<T>();
        }
        
        public IProjectionRepository<T> Build<T>(string reference)
        {
            return new MemoryRepository<T>();
        }
    }
}
