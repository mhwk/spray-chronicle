using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryPipeline : IPipeline, IMailStrategyRouterSubscriber<IExecute>
    {
        
    }
}
