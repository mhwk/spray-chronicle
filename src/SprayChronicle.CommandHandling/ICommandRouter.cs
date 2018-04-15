using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public interface ICommandRouter
    {
        Task Route(params object[] commands);
    }
}
