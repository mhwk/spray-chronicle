using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SprayChronicle.MessageHandling
{
    public interface IMessageSource<out T> : ISourceBlock<T>
    {
        Task Start();
        Task Stop();
    }
}
