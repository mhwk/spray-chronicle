using System.Threading.Tasks;

namespace SprayChronicle.EventHandling
{
    public interface IHandleStream
    {
        Task ListenAsync();
    }
}
