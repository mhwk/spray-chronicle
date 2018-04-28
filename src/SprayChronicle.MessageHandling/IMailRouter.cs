using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IMailRouter
    {
        Task<object> Route(IEnvelope envelope);
    }
}
