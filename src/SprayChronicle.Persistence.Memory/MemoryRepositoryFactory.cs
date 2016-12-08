using SprayChronicle.QueryHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryRepositoryFactory : IBuildStatefulRepositories
    {
        public IStatefulRepository<T> Build<T>()
        {
            return new MemoryRepository<T>();
        }
        
        public IStatefulRepository<T> Build<T>(string reference)
        {
            return new MemoryRepository<T>();
        }
    }
}
