using System.Threading;
using System.Threading.Tasks;

namespace SprayChronicle
{
    public interface IBackgroundTask
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
