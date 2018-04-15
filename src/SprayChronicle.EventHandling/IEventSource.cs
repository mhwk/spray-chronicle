using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SprayChronicle.EventHandling
{
    public interface IEventSource<out T> : ISourceBlock<T>
    {
        Task Start();
    }
}
