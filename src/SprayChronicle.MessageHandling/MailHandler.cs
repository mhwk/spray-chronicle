using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public delegate Task<object> MailHandler(IEnvelope envelope);
}
