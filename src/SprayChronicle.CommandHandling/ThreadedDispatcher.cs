using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public class ThreadedDispatcher : IDispatchCommand
    {
        private readonly IDispatchCommand _internalDispatcher;

        public ThreadedDispatcher(IDispatchCommand internalDispatcher)
        {
            _internalDispatcher = internalDispatcher;
        }

        public async Task Dispatch(object command)
        {
            await Task.Run(() => _internalDispatcher.Dispatch(command));
        }
    }
}
