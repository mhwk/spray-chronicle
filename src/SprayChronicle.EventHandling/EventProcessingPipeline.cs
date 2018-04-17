using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventHandling
{
    public abstract class EventProcessingPipeline<TProcessor,TMessage> : IPipeline
        where TProcessor : class
    {
        public string Description => $"EventProcessing: {typeof(TProcessor).Name}";
        
        private readonly ISourceBlock<TMessage> _source;
        
        private readonly TProcessor _processor;
        
        private readonly IMessagingStrategy<TProcessor> _strategy = new OverloadMessagingStrategy<TProcessor>(new ContextTypeLocator<TProcessor>());
        
        protected EventProcessingPipeline(
            ISourceBlock<TMessage> source,
            TProcessor processor)
        {
            _source = source;
            _processor = processor;
        }
        
        public Task Start()
        {
            var domain = new TransformBlock<TMessage,DomainMessage>(message => Transform(message));
            var routed = new TransformBlock<DomainMessage,Processed>(message => Apply(message));
            var batched = new BatchBlock<Processed>(1000);
            var action = new ActionBlock<Processed[]>(processed => Apply(processed));

            _source.LinkTo(domain, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            domain.LinkTo(routed, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            routed.LinkTo(batched, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            routed.LinkTo(batched, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            batched.LinkTo(action, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            return _source.Completion;
        }

        protected abstract Task<DomainMessage> Transform(TMessage message);

        private async Task<Processed> Apply(DomainMessage domainMessage)
        {
            return await _strategy
                .Ask<Processed>(_processor, domainMessage.Payload, domainMessage.Epoch)
                .ConfigureAwait(false);
        }

        protected abstract Task Apply(Processed[] processed);

    }
}
