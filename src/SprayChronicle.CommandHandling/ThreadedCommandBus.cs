using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public class ThreadedCommandBus : IDispatchCommands
    {
        readonly IDispatchCommands _internalDispatcher;

        public ThreadedCommandBus(IDispatchCommands internalDispatcher)
        {
            _internalDispatcher = internalDispatcher;
        }

        public async void Dispatch(object command)
        {
            await Task.Run(() => _internalDispatcher.Dispatch(command));
        }
    }
}
