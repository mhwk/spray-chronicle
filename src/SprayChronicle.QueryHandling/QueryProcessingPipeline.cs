using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.QueryHandling
{
    public sealed class QueryProcessingPipeline<TProcessor> : IPipeline
        where TProcessor : class
    {
        public string Description => $"Query processing: {typeof(TProcessor).Name}";

        private const int BatchSize = 1000;
        private const int BatchTimeout = 100;
        private const int Parallelism = 4;
        
        private readonly IMailStrategy<TProcessor> _strategy = new OverloadMailStrategy<TProcessor>(new ContextTypeLocator<TProcessor>());
        
        private readonly ILogger<TProcessor> _logger;
        
        private readonly IEventSourceFactory _sourceFactory;
        
        private readonly CatchUpOptions _sourceOptions;

        private readonly IQueryProcessingAdapter _handler;

        private readonly TProcessor _processor;
        
        private IEventSource<TProcessor> _source;

        public QueryProcessingPipeline(
            ILogger<TProcessor> logger,
            IEventSourceFactory sourceFactory,
            CatchUpOptions sourceOptions,
            IQueryProcessingAdapter handler,
            TProcessor processor)
        {
            _logger = logger;
            _sourceFactory = sourceFactory;
            _sourceOptions = sourceOptions;
            _handler = handler;
            _processor = processor;
        }
        
        public async Task Start()
        {
            if (null != _source) {
                throw new Exception("Query processing already started");
            }
            
            _source = _sourceFactory.Build<TProcessor,CatchUpOptions>(
                _sourceOptions.WithCheckpoint(await _handler.Checkpoint())
            );
            
            var converted = new TransformBlock<object,EventEnvelope>(
                message => {
                    try {
                        return _source.Convert(_strategy, message);
                    } catch (Exception error) {
//                        _logger.LogCritical(error);
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize
                }
            );
            var routed = new TransformBlock<EventEnvelope,Processed>(
                async message => {
                    if (null == message) return null;
                    
                    try {
                        return await _strategy.Ask<Processed>(_processor, message.Message, message.Epoch);
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        throw;
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize
                }
            );
            var batched = new BatchBlock<Processed>(BatchSize, new GroupingDataflowBlockOptions {
//                Greedy = true,
                BoundedCapacity = BatchSize
            });
            var action = new ActionBlock<Processed[]>(
                async processed => {
                    var measure = new MeasureMilliseconds();
                    try {
                        await _handler.Apply(processed);
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        throw;
                    } finally {
                        _logger.LogInformation($"Applied {processed.Length} messages in {measure.Stop()}");
                    }
                }
            );

            var timer = new Timer(time => {
                batched.TriggerBatch();
            });
            
            var timeout = new TransformBlock<Processed,Processed>(
                processed => {
                    timer.Change(TimeSpan.FromMilliseconds(BatchTimeout), Timeout.InfiniteTimeSpan);
                    return processed;
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize
                }
            );
            
            _source.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(routed, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            routed.LinkTo(timeout, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            timeout.LinkTo(batched, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            batched.LinkTo(action, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await _source.Start();
            await _source.Completion;
            await converted.Completion;
            await routed.Completion;
            await timeout.Completion;
            await batched.Completion;
            await action.Completion;
        }

        public Task Stop()
        {
            if (null == _source) {
                throw new Exception("Query processing not started");
            }
            
            _source.Complete();
            return _source.Completion;
        }
    }
}
