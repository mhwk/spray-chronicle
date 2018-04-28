using System;
using System.Threading.Tasks;

namespace SprayChronicle.CommandHandling
{
    public sealed class RouterCommandDispatcher : ICommandDispatcher
    {
        private readonly CommandRouter _router;

        public RouterCommandDispatcher(CommandRouter router)
        {
            _router = router;
        }

        public async Task Dispatch(object command)
        {
            var onComplete = new TaskCompletionSource<object>();
            
            await _router.Route(new CommandEnvelope(
                Guid.NewGuid().ToString(),
                null,
                Guid.NewGuid().ToString(),
                command,
                DateTime.Now,
                () => onComplete.TrySetResult(null),
                error => onComplete.TrySetException(error)
            ));

            await onComplete.Task;
        }
    }
}
