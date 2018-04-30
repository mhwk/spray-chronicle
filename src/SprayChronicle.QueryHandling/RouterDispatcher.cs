using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public sealed class RouterQueryDispatcher : IQueryDispatcher
    {
        private readonly QueryRouter _router;

        public RouterQueryDispatcher(QueryRouter router)
        {
            _router = router;
        }

        
        public async Task<object> Dispatch(params object[] queries)
        {
            var tasks = new List<Task>();

            foreach (var query in queries) {
                var completion = new TaskCompletionSource<object>();
            
                await _router.Route(new QueryEnvelope(
                    Guid.NewGuid().ToString(),
                    null,
                    Guid.NewGuid().ToString(),
                    query,
                    DateTime.Now,
                    result => completion.TrySetResult(result),
                    error => completion.TrySetException(error)
                ));

                tasks.Add(completion.Task);
            }

            return Task.WhenAll(tasks);
        }
    }
}
