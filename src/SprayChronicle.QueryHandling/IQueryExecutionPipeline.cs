using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public interface IQueryExecutionPipeline : IPipeline, IMailStrategyRouterSubscriber<IExecute>
    {
        
    }
}
