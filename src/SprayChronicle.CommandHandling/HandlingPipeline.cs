using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public sealed class HandlingPipeline<THandler,TState> : ICommandHandlingPipeline
        where THandler : class, IHandle
        where TState : class, IEventSourcable<TState>
    {
        public string Description => $"CommandHandling: {typeof(THandler).Name}";
        
        private readonly IMessagingStrategy<THandler> _strategy = new OverloadMessagingStrategy<THandler>("Handle");

        private readonly BufferBlock<object> _queue;

        private readonly THandler _handler;
        
        private readonly IEventSourcingRepository<TState> _repository;

        public HandlingPipeline(
            IEventSourcingRepository<TState> repository,
            THandler handler) :  this(
                repository,
                handler,
                new BufferBlock<object>()
            )
        {
        }

        public HandlingPipeline(
            IEventSourcingRepository<TState> repository,
            THandler handler,
            BufferBlock<object> queue)
        {
            _repository = repository;
            _handler = handler;
            _queue = queue;
        }

        public void Subscribe(IMessagingStrategyRouter<IHandle> messageRouter)
        {
            messageRouter.Subscribe(_strategy, command => {
                _queue.Post(command);
                return Task.FromResult<object>(null);
            });
        }

        public async Task Start()
        {
            var dispatch = new TransformBlock<object,Handled>(command => Dispatch(command));
            var apply = new ActionBlock<Handled>(command => Apply(command));

            _queue.LinkTo(dispatch, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            dispatch.LinkTo(apply, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await Task.WhenAll(dispatch.Completion);
        }

        public async Task Stop()
        {
            _queue.Complete();
            await _queue.Completion;
        }

        private Task<Handled> Dispatch(object command)
        {
            return _strategy.Ask<Handled>(_handler, command);
        }

        private async Task Apply(Handled handled)
        {
            TState identity;
            
            if (null != handled.Identity) {
                identity = await handled.Do(await _repository.Load<TState>(handled.Identity)) as TState;
            } else {
                identity = await handled.Do() as TState;
            }

            await _repository.Save<TState>(identity);
        }
    }
}
