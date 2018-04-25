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
        public string Description => $"CommandProcessor: {typeof(THandler).Name}";
        
        private readonly IMessagingStrategy<THandler> _strategy = new OverloadMessagingStrategy<THandler>("Process");

        private readonly ILogger<THandler> _logger;
        
        private readonly IEventSourceFactory _sourceFactory;
        
        private readonly PersistentOptions _sourceOptions;

        private readonly IMessageRouter _router;

        private readonly THandler _handler;

        private IEventSource<THandler> _source;

        public ProcessingPipeline(
            ILogger<THandler> logger,
            IEventSourceFactory sourceFactory,
            PersistentOptions sourceOptions,
            IMessageRouter router,
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
            
            _source = _sourceFactory.Build<THandler,PersistentOptions>(_sourceOptions);
            var converted = new TransformBlock<object,DomainMessage>(message => {
                try {
                    return _source.Convert(_strategy, message);
                } catch (UnsupportedMessageException error) {
                    _logger.LogDebug(error);
                    return null;
                }
            });
            var dispatch = new TransformBlock<DomainMessage,Processed>(message => Dispatch(message));
            var apply = new ActionBlock<Processed>(command => Apply(command));

            _source.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(dispatch, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            dispatch.LinkTo(apply, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            
            await Task.WhenAll(_source.Start(), dispatch.Completion);
        }

        public Task Stop()
        {
            if (null == _source) {
                throw new PipelineException($"Command processing pipeline not running");
            }
            
            _source.Complete();
            
            return Task.CompletedTask;
        }

        private async Task<Processed> Dispatch(DomainMessage message)
        {
            if (null == message) {
                return await Processed.WithError(new UnhandledCommandException(
                    "Command not handled by pipeline"
                ));
            }
            
            try {
                return await _strategy.Ask<Processed>(_handler, message.Payload, message.Epoch);
            } catch (Exception error) {
                _logger.LogDebug(error);
                return await Processed.WithError(error);
            }
        }
        
        private async Task Apply(Processed processed)
        {
            if (!(processed is ProcessedDispatch dispatch)) {
                throw new ArgumentException($"Processed is expected to be a {typeof(ProcessedDispatch)}, {processed.GetType()} given");
            }

            await _router.Route(dispatch.Command);
        }
    }
}
