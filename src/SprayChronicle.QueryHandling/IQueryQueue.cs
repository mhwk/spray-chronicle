using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryQueue : IEventSource<QueryRequest>, ITargetBlock<QueryRequest>
    {
        
    }
}
