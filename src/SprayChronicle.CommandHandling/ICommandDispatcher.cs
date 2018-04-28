using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public interface ICommandDispatcher
    {
        Task Dispatch(object command);
    }
}
