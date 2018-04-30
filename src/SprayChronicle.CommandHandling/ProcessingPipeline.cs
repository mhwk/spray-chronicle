using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;

namespace SprayChronicle.CommandHandling
{
    public sealed class ProcessingPipeline<THandler> : IPipeline
        where THandler : class, IProcess
    {
        private const int Parallelism = 2;
        private const int BatchSize = 100;
        
        public string Description => $"CommandProcessor: {typeof(THandler).Name}";
        
        private readonly IMailStrategy<THandler> _strategy = new OverloadMailStrategy<THandler>("Process");

        private readonly ILogger<THandler> _logger;
        
        private readonly IEventSourceFactory _sourceFactory;
        
        private readonly CatchUpOptions _sourceOptions;

        private readonly IMailRouter _router;

        private readonly THandler _handler;

        private IEventSource<THandler> _source;

        public ProcessingPipeline(
            ILogger<THandler> logger,
            IEventSourceFactory sourceFactory,
            CatchUpOptions sourceOptions,
            IMailRouter router,
            THandler handler)
        {
            _logger = logger;
            _sourceFactory = sourceFactory;
            _sourceOptions = sourceOptions;
            _router = router;
            _handler = handler;
        }

        public async Task Start()
        {
            if (null != _source) {
                throw new PipelineException($"Command processing pipeline already started");
            }
            
            _source = _sourceFactory.Build<THandler,CatchUpOptions>(_sourceOptions);
            var converted = new TransformBlock<object,DomainEnvelope>(
                message => {
                    try {
                        var result = _source.Convert(_strategy, message);
                        return result;
                    } catch (UnsupportedMessageException error) {
//                        _logger.LogDebug(error);
                        return null;
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize * Parallelism
                }
            );
            var dispatch = new TransformBlock<DomainEnvelope,Tuple<DomainEnvelope,Processed>>(
                async envelope => {
                    if (null == envelope) return null;
                    
                    try {
                        return new Tuple<DomainEnvelope, Processed>(
                            envelope,
                            await Process(envelope)
                        );
                    } catch (Exception error) {
                        _logger.LogError(error);
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize * Parallelism
                }
            );
            var apply = new ActionBlock<Tuple<DomainEnvelope,Processed>>(
                async tuple => {
                    if (null == tuple) return;
                    
                    try {
                        await Apply(tuple.Item1, tuple.Item2);
                    } catch (Exception error) {
                        _logger.LogCritical(error);
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    BoundedCapacity = BatchSize * Parallelism
                }
            );

            _source.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(dispatch, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            dispatch.LinkTo(apply, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await _source.Start();
            await converted.Completion;
            await dispatch.Completion;
            await apply.Completion;
        }
        

        public Task Stop()
        {
            if (null == _source) {
                throw new PipelineException($"Command processing pipeline not running");
            }
            
            _source.Complete();
            
            return _source.Completion;
        }

        private async Task<Processed> Process(DomainEnvelope envelope)
        {
            return await _strategy.Ask<Processed>(_handler, envelope.Message, envelope.Epoch);
        }
        
        private async Task Apply(DomainEnvelope envelope, Processed processed)
        {
            if (!(processed is ProcessedDispatch dispatch)) {
                throw new ArgumentException($"Processed is expected to be a {typeof(ProcessedDispatch)}, {processed.GetType()} given");
            }

            var completion = new TaskCompletionSource<object>();
            
            await _router.Route(new CommandEnvelope(
                GuidUtility.Create(Guid.Parse(envelope.MessageId), envelope.CorrelationId).ToString(),
                envelope.MessageId,
                envelope.CorrelationId,
                dispatch.Command,
                DateTime.Now,
                result => completion.TrySetResult(null),
                error => completion.TrySetException(error)
            ));
            
            await completion.Task;
            
            _logger.LogDebug($"Dispatched {dispatch.Command.GetType()} in response to {envelope.MessageName}");
        }
    }
}
