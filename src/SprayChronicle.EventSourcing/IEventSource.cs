using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public interface IEventSource<out TTarget> : ISourceBlock<object>
        where TTarget : class
    {
        Task Start();

        DomainMessage Convert(IMessagingStrategy<TTarget> strategy, object message);
    }
}
