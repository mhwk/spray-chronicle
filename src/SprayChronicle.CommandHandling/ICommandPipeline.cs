using SprayChronicle.EventHandling;

namespace SprayChronicle.CommandHandling
{
    public interface ICommandPipeline : IPipeline, ICommandRouterSubscriber
    {
        
    }
}
