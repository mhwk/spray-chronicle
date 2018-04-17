using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public sealed class ProcessedFactory
    {
        public Task<ProcessedDispatch> Dispatch(object command)
        {
            return Task.FromResult(new ProcessedDispatch(command));
        }
    }
}