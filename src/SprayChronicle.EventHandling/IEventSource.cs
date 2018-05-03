using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public interface IEventSource
    {
        
    }
    
    public interface IEventSource<out TTarget> : ISourceBlock<object>, IEventSource
        where TTarget : class
    {
        Task Start();
        
        EventEnvelope Convert(IMailStrategy<TTarget> strategy, object message);
    }
}
