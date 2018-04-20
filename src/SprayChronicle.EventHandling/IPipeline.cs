using System.Threading;
using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public interface IPipeline
    {
        string Description { get; }

        Task Start();
        Task Stop();
    }
}
