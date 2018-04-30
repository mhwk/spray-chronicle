using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public interface ICommandDispatcher
    {
        Task Dispatch(params object[] command);
    }
}
