using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public abstract class ExecutionPipeline<TSession,TQueryExecutor> : IQueryPipeline
        where TQueryExecutor : class, IExecute
    {
        public string Description => $"QueryExecution: {typeof(TQueryExecutor).Name}";
        
        private readonly BufferBlock<QueryRequest> _queue = new BufferBlock<QueryRequest>();
        
        private readonly IMessagingStrategy<TQueryExecutor> _strategy = new OverloadMessagingStrategy<TQueryExecutor>("Execute");
        
        private readonly TQueryExecutor _processor;

        protected ExecutionPipeline(TQueryExecutor processor)
        {
            _processor = processor;
        }

        public async Task Start()
        {
            var executed = new TransformBlock<QueryRequest,Tuple<Action<object>,Executor>>(
                async request => new Tuple<Action<object>,Executor>(
                    request.OnSuccess,
                    await ExecuteQuery(request)
                )
            );
            var applied = new TransformBlock<Tuple<Action<object>,Executor>,Tuple<Action<object>,object>>(
                async tuple => new Tuple<Action<object>,object>(
                    tuple.Item1,
                    await Apply(tuple.Item2)
                )
            );
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
            _queue.Complete();
            return Task.CompletedTask;
        }

        private async Task<Executor> ExecuteQuery(QueryRequest query)
        {
            return await _strategy.Ask<Executor>(_processor, query).ConfigureAwait(false);
        }

        protected abstract Task<object> Apply(Executor executor);
        
        public void Subscribe(IMessagingStrategyRouter<IExecute> messageRouter)
        {
            messageRouter.Subscribe(_strategy as IMessagingStrategy<IExecute>, arguments =>
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
