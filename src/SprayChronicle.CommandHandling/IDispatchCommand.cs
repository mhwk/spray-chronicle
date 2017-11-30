using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public interface IDispatchCommand
    {
        Task Dispatch(object command);
    }
}
