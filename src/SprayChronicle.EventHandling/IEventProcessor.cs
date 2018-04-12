using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public interface IEventProcessor
    {
        Task Process();
    }
}
