using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public class ThreadedDispatcher : IDispatchCommand
    {
        readonly IDispatchCommand _internalDispatcher;

        public ThreadedDispatcher(IDispatchCommand internalDispatcher)
        {
            _internalDispatcher = internalDispatcher;
        }

        public async void Dispatch(object command)
        {
            await Task.Run(() => _internalDispatcher.Dispatch(command));
        }
    }
}
