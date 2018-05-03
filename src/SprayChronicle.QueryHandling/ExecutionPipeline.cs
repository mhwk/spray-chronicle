using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public sealed class ExecutionPipeline<TQueryExecutor> : IQueryExecutionPipeline
        where TQueryExecutor : class, IExecute
    {
        public string Description => $"QueryExecution: {typeof(TQueryExecutor).Name}";
        
        private readonly BufferBlock<QueryEnvelope> _queue = new BufferBlock<QueryEnvelope>();
        
        private readonly IMailStrategy<TQueryExecutor> _strategy = new OverloadMailStrategy<TQueryExecutor>("Execute");

        private readonly ILogger<TQueryExecutor> _logger;
        
        private readonly TQueryExecutor _processor;
        
        private readonly IQueryExecutionAdapter _adapter;

        public ExecutionPipeline(
            ILogger<TQueryExecutor> logger,
            TQueryExecutor processor,
            IQueryExecutionAdapter adapter)
        {
            _logger = logger;
            _processor = processor;
            _adapter = adapter;
        }

        public async Task Start()
        {
            _logger.LogDebug("Starting execution pipeline...");
            var executed = new TransformBlock<QueryEnvelope,Tuple<QueryEnvelope,Executed>>(
                async request => {
                    try {
                        return new Tuple<QueryEnvelope, Executed>(
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
            var applied = new TransformBlock<Tuple<QueryEnvelope,Executed>,Tuple<QueryEnvelope,object>>(
                async tuple => {
                    try {
                        var result = await _adapter.Apply(tuple.Item2);
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

        private async Task<Executed> ExecuteQuery(QueryEnvelope query)
        {
            return await _strategy.Ask<Executed>(_processor, query.Message, query.Epoch);
        }
        
        public void Subscribe(IMailStrategyRouter<IExecute> messageRouter)
        {
            messageRouter.Subscribe(
                _strategy,
                async envelope => await _queue.SendAsync((QueryEnvelope) envelope)
            );
        }
    }
}
