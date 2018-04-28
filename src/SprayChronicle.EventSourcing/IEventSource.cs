using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public interface IEventSource
    {
        
    }
    
    public interface IEventSource<out TTarget> : ISourceBlock<object>, IEventSource
        where TTarget : class
    {
        Task Start();
        
        DomainEnvelope Convert(IMailStrategy<TTarget> strategy, object message);
    }
}
