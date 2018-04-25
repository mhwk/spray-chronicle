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
        
        private readonly BufferBlock<QueryEnvelope> _queue = new BufferBlock<QueryEnvelope>();
        
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
            var executed = new TransformBlock<QueryEnvelope,Tuple<QueryEnvelope,Executor>>(
                async request => {
                    try {
                        return new Tuple<QueryEnvelope, Executor>(
                            request,
                            await ExecuteQuery(request)
                        );
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        request.OnError(error);
                        return null;
                    }
                });
            var applied = new TransformBlock<Tuple<QueryEnvelope,Executor>,Tuple<QueryEnvelope,object>>(
                async tuple => {
                    try {
                        return new Tuple<QueryEnvelope, object>(
                            tuple.Item1,
                            await Apply(tuple.Item2)
                        );
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        tuple.Item1.OnError(error);
                        return null;
                    }
                });
            var succeeded = new ActionBlock<Tuple<QueryEnvelope,object>>(
                tuple => tuple.Item1.OnSuccess(tuple.Item2)
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
            
            await applied.Completion;
        }

        public Task Stop()
        {
            _logger.LogDebug("Stopping execution pipeline...");
            _queue.Complete();
            return Task.CompletedTask;
        }

        private async Task<Executor> ExecuteQuery(QueryEnvelope query)
        {
            return await _strategy.Ask<Executor>(_processor, query.Queries).ConfigureAwait(false);
        }

        protected abstract Task<object> Apply(Executor executor);
        
        public void Subscribe(IMessagingStrategyRouter<IExecute> messageRouter)
        {
            messageRouter.Subscribe(_strategy, arguments =>
            {
                var completion = new TaskCompletionSource<object>();

                _queue.Post(new QueryEnvelope(
                    arguments,
                    result => completion.TrySetResult(result),
                    error => completion.TrySetException(error)
                ));

                return completion.Task;
            });
        }
    }
}
