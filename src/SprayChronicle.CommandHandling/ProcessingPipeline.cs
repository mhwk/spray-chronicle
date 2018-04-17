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

        private readonly ISourceBlock<DomainMessage> _queue;
        
        private readonly IMessageRouter _router;
        
        private readonly THandler _handler;

        public ProcessingPipeline(
            ISourceBlock<DomainMessage> queue,
            IMessageRouter router,
            THandler handler)
        {
            _queue = queue;
            _router = router;
            _handler = handler;
        }

        public async Task Start()
        {
            var dispatch = new TransformBlock<DomainMessage,Processed>(message => Dispatch(message));
            var apply = new ActionBlock<Processed>(command => Apply(command));

            _queue.LinkTo(dispatch, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            dispatch.LinkTo(apply, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await apply.Completion;
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
