using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public interface IPipelineManager
    {
        IPipelineManager Attach(IPipeline pipeline);
        Task Start();
        Task Stop();
    }
}
