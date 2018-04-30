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
        
        private readonly IMailStrategy<TQueryExecutor> _strategy = new OverloadMailStrategy<TQueryExecutor>("Execute");

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
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = 4
                }
            );
            var applied = new TransformBlock<Tuple<QueryEnvelope,Executor>,Tuple<QueryEnvelope,object>>(
                async tuple => {
                    try {
                        var result = await Apply(tuple.Item2);
                        return new Tuple<QueryEnvelope, object>(
                            tuple.Item1,
                            result
                        );
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        tuple.Item1.OnError(error);
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = 4
                }
            );
            var succeeded = new ActionBlock<Tuple<QueryEnvelope,object>>(
                tuple => tuple.Item1.OnSuccess(tuple.Item2),
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = 4
                }
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

            await _queue.Completion;
            await executed.Completion;
            await applied.Completion;
            await succeeded.Completion;
        }

        public Task Stop()
        {
            _logger.LogDebug("Stopping execution pipeline...");
            _queue.Complete();
            return _queue.Completion;
        }

        private async Task<Executor> ExecuteQuery(QueryEnvelope query)
        {
            return await _strategy.Ask<Executor>(_processor, query.Message, query.Epoch);
        }

        protected abstract Task<object> Apply(Executor executor);
        
        public void Subscribe(IMailStrategyRouter<IExecute> messageRouter)
        {
            messageRouter.Subscribe(
                _strategy,
                async envelope => await _queue.SendAsync((QueryEnvelope) envelope)
            );
        }
    }
}
