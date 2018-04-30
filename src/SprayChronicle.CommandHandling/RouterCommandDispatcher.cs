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

        public async Task Dispatch(params object[] commands)
        {
            foreach (var command in commands) {
                var completion = new TaskCompletionSource<object>();
            
                await _router.Route(new CommandEnvelope(
                    Guid.NewGuid().ToString(),
                    null,
                    Guid.NewGuid().ToString(),
                    command,
                    DateTime.Now,
                    result => completion.TrySetResult(null),
                    error => completion.TrySetException(error)
                ));

                await completion.Task;
            }
        }
    }
}
