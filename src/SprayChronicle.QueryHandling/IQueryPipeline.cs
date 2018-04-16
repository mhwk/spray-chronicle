using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryPipeline : IPipeline, IRouterSubscriber<IExecute>, IRouterSubscriber<IProcess>
    {
        
    }
}
