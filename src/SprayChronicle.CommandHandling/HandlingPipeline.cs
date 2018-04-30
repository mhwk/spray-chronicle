using System;
using System.Linq;
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
        
        private readonly IMailStrategy<THandler> _strategy = new OverloadMailStrategy<THandler>("Handle");

        private readonly BufferBlock<CommandEnvelope> _queue;

        private readonly THandler _handler;
        
        private readonly IEventSourcingRepository<TState> _repository;

        public HandlingPipeline(
            IEventSourcingRepository<TState> repository,
            THandler handler) :  this(
                repository,
                handler,
                new BufferBlock<CommandEnvelope>()
            )
        {
        }

        public HandlingPipeline(
            IEventSourcingRepository<TState> repository,
            THandler handler,
            BufferBlock<CommandEnvelope> queue)
        {
            _repository = repository;
            _handler = handler;
            _queue = queue;
        }

        public void Subscribe(IMailStrategyRouter<IHandle> messageRouter)
        {
            messageRouter.Subscribe(
                _strategy,
                async envelope => await _queue.SendAsync((CommandEnvelope) envelope)
            );
        }

        public async Task Start()
        {
            var dispatch = new TransformBlock<CommandEnvelope,Tuple<CommandEnvelope,Handled>>(
                async envelope => {
                    try {
                        return new Tuple<CommandEnvelope, Handled>(
                            envelope,
                            await Dispatch(envelope)
                        );
                    } catch (Exception error) {
                        envelope.OnError(error);
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = 4
                }
            );
            var apply = new ActionBlock<Tuple<CommandEnvelope,Handled>>(
                async tuple => {
                    if (null == tuple) return;
                    
                    try {
                        await Apply(tuple.Item1, tuple.Item2);
                        tuple.Item1.OnSuccess(null);
                    } catch (Exception error) {
                        tuple.Item1.OnError(error);
                    }
                },
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = 4
                }
            );

            _queue.LinkTo(dispatch, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            dispatch.LinkTo(apply, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await _queue.Completion;
            await dispatch.Completion;
            await apply.Completion;
        }

        public async Task Stop()
        {
            _queue.Complete();
            await _queue.Completion;
        }

        private Task<Handled> Dispatch(CommandEnvelope envelope)
        {
            return _strategy.Ask<Handled>(_handler, envelope.Message, envelope.Epoch);
        }

        private async Task Apply(CommandEnvelope envelope, Handled handled)
        {
            TState identity;

            switch (handled) {
                case HandledCreate<TState> created:
                    identity = await created.Do();
                    break;
                case HandledUpdate<TState> updated:
                    identity = await updated.Do(await _repository.Load<TState>(handled.Identity, envelope.MessageId));
                    break;
                default:
                    throw new Exception($"Unsupported pipeline result {handled.GetType()}");
            }
            
            await _repository.Save<TState>(identity, envelope);
        }
    }
}
