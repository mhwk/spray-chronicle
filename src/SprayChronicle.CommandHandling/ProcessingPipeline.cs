using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public sealed class ProcessingPipeline<THandler> : IPipeline
        where THandler : class
    {
        public string Description => $"CommandProcessor: {typeof(THandler).Name}";
        
        private readonly IMessagingStrategy<THandler> _strategy = new OverloadMessagingStrategy<THandler>("Process");

        private readonly IEventSourceFactory _sourceFactory;
        
        private readonly PersistentOptions _sourceOptions;

        private readonly IMessageRouter _router;
        
        private readonly THandler _handler;

        public ProcessingPipeline(
            IEventSourceFactory sourceFactory,
            PersistentOptions sourceOptions,
            IMessageRouter router,
            THandler handler)
        {
            _sourceFactory = sourceFactory;
            _sourceOptions = sourceOptions;
            _router = router;
            _handler = handler;
        }

        public async Task Start()
        {
            var source = _sourceFactory.Build<THandler,PersistentOptions>(_sourceOptions);
            var converted = new TransformBlock<object,DomainMessage>(message => source.Convert(_strategy, message));
            var dispatch = new TransformBlock<DomainMessage,Processed>(message => Dispatch(message));
            var apply = new ActionBlock<Processed>(command => Apply(command));

            source.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(dispatch, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            dispatch.LinkTo(apply, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await Task.WhenAll(source.Start(), apply.Completion);
        }

        private Task<Processed> Dispatch(DomainMessage message)
        {
            return _strategy.Ask<Processed>(_handler, message);
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
