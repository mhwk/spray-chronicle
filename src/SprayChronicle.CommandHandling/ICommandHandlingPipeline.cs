using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public interface ICommandHandlingPipeline : IPipeline, IMailStrategyRouterSubscriber<IHandle>
    {
        
    }
}
