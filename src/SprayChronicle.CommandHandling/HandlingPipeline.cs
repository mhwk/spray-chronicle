﻿using System;
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
        
        private readonly IMessagingStrategy<THandler> _strategy = new OverloadMessagingStrategy<THandler>("Handle");

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

        public void Subscribe(IMessagingStrategyRouter<IHandle> messageRouter)
        {
            messageRouter.Subscribe(_strategy, commands => {
                var onComplete = new TaskCompletionSource<object>();
                
                _queue.Post(new CommandEnvelope(
                    commands.First(),
                    () => onComplete.TrySetResult(new object()),
                    error => onComplete.TrySetException(error)
                ));

                return onComplete.Task;
            });
        }

        public async Task Start()
        {
            var dispatch = new TransformBlock<CommandEnvelope,Tuple<CommandEnvelope,Handled>>(async envelope => {
                try {
                    return new Tuple<CommandEnvelope, Handled>(
                        envelope,
                        await Dispatch(envelope.Command)
                    );
                } catch (Exception error) {
                    envelope.OnError(error);
                    return null;
                }
            });
            var apply = new ActionBlock<Tuple<CommandEnvelope,Handled>>(async tuple => {
                if (null == tuple) {
                    return;
                }
                
                try {
                    await Apply(tuple.Item2);
                    tuple.Item1.OnSuccess();
                } catch (Exception error) {
                    tuple.Item1.OnError(error);
                }
            });

            _queue.LinkTo(dispatch, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            dispatch.LinkTo(apply, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await dispatch.Completion;
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

            switch (handled) {
                case HandledCreate<TState> created:
                    identity = await created.Do();
                    break;
                case HandledUpdate<TState> updated:
                    identity = await updated.Do(await _repository.Load<TState>(handled.Identity));
                    break;
                default:
                    throw new Exception($"Unsupported pipeline result {handled.GetType()}");
            }
            
            await _repository.Save<TState>(identity);
        }
    }
}
