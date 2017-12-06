using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public class ThreadedDispatcher : IDispatchCommands
    {
        private readonly IDispatchCommands _internalDispatcher;

        public ThreadedDispatcher(IDispatchCommands internalDispatcher)
        {
            _internalDispatcher = internalDispatcher;
        }

        public async Task Dispatch(object command)
        {
            await Task.Run(() => _internalDispatcher.Dispatch(command));
        }
    }
}
