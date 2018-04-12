using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public interface IHandleCommands
    {
        Task Handle(object command);
    }
}
