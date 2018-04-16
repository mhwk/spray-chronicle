using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public interface ICommandPipeline : IPipeline, IRouterSubscriber<IHandle>, IRouterSubscriber<IProcess>
    {
        
    }
}
