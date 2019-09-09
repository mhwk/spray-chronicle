using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IHandleCommand
    {
        Task Handle(object cmd, string messageId = null);
    }
}
