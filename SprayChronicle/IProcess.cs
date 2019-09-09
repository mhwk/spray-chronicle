using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IProcess
    {
        Task Process(Envelope<object> envelope);
    }
}
