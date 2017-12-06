using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryRepositoryFactory : IBuildStatefulRepositories
    {
        public IStatefulRepository<T> Build<T>() where T : class
        {
            return new MemoryRepository<T>();
        }
        
        public IStatefulRepository<T> Build<T>(string reference) where T : class
        {
            return new MemoryRepository<T>();
        }
    }
}
