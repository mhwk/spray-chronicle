using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public interface IDispatchCommands
    {
        Task Dispatch(params object[] commands);
    }
}
