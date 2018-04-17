using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public interface IMessageRouter
    {
        Task<object> Route(params object[] arguments);
    }
}
