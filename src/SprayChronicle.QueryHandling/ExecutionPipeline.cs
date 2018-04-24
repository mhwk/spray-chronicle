using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public abstract class ExecutionPipeline<TQueryExecutor> : IQueryPipeline
        where TQueryExecutor : class, IExecute
    {
        public string Description => $"QueryExecution: {typeof(TQueryExecutor).Name}";
        
        private readonly BufferBlock<QueryRequest> _queue = new BufferBlock<QueryRequest>();
        
        private readonly IMessagingStrategy<TQueryExecutor> _strategy = new OverloadMessagingStrategy<TQueryExecutor>("Execute");

        private readonly ILogger<TQueryExecutor> _logger;
        
        private readonly TQueryExecutor _processor;

        protected ExecutionPipeline(
            ILogger<TQueryExecutor> logger,
            TQueryExecutor processor)
        {
            _logger = logger;
            _processor = processor;
        }

        public async Task Start()
        {
            _logger.LogDebug("Starting execution pipeline...");
            var executed = new TransformBlock<QueryRequest,Tuple<Action<object>,Executor>>(
                async request => {
                    try {
                        return new Tuple<Action<object>, Executor>(
                            request.OnSuccess,
                            await ExecuteQuery(request)
                        );
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        // @todo handle error
                        return null;
                    }
                });
            var applied = new TransformBlock<Tuple<Action<object>,Executor>,Tuple<Action<object>,object>>(
                async tuple => {
                    try {
                        return new Tuple<Action<object>, object>(
                            tuple.Item1,
                            await Apply(tuple.Item2)
                        );
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        // @todo handle error
                        return null;
                    }
                });
            var succeeded = new ActionBlock<Tuple<Action<object>,object>>(
                tuple => tuple.Item1(tuple.Item2)
            );

            _queue.LinkTo(executed, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            executed.LinkTo(applied, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            applied.LinkTo(succeeded, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            
            await Task.WhenAll(applied.Completion);
        }

        public Task Stop()
        {
            _logger.LogDebug("Stopping execution pipeline...");
            _queue.Complete();
            return Task.CompletedTask;
        }

        private async Task<Executor> ExecuteQuery(QueryRequest query)
        {
            return await _strategy.Ask<Executor>(_processor, query.Payload).ConfigureAwait(false);
        }

        protected abstract Task<object> Apply(Executor executor);
        
        public void Subscribe(IMessagingStrategyRouter<IExecute> messageRouter)
        {
            messageRouter.Subscribe(_strategy, arguments =>
            {
                var onComplete = new TaskCompletionSource<object>();

                _queue.Post(new QueryRequest(
                    arguments,
                    result => onComplete.TrySetResult(result)
                ));

                return onComplete.Task;
            });
        }
    }
}
