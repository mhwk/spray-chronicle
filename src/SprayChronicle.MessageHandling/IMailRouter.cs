using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IMailRouter
    {
        Task Route(IEnvelope envelope);
    }
}
