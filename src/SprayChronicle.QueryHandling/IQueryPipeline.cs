using SprayChronicle.EventHandling;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryPipeline : IQueryExecutor, IEventProcessor
    {
        
    }
}
