using SprayChronicle.EventHandling;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryProcessor : IQueryExecutor, IEventProcessor
    {
        IQueryScope[] Dequeue { get; }
    }
}
