using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public abstract class MailStrategyRouter<THandler> : IMailRouter, IMailStrategyRouter<THandler>
        where THandler : class
    {
        private readonly int _maxNumberOfMatches;
        
        private readonly Dictionary<IMailStrategy,MailHandler> _strategies = new Dictionary<IMailStrategy,MailHandler>();

        protected MailStrategyRouter(int maxNumberOfMatches = int.MaxValue)
        {
            _maxNumberOfMatches = maxNumberOfMatches;
        }

        public IMailStrategyRouter<THandler> Subscribe(IMailStrategy strategy, MailHandler handler)
        {
            if (_strategies.ContainsKey(strategy)) {
                throw new Exception($"Strategy {strategy.GetType()} already subscribed");
            }
            
            _strategies.Add(strategy, handler);

            return this;
        }

        public IMailStrategyRouter<THandler> Subscribe(IMailStrategyRouterSubscriber<THandler> subscriber)
        {
            subscriber.Subscribe(this);
            return this;
        }
        
        public async Task Route(IEnvelope envelope)
        {
            var tasks = new List<Task>();
            var results = new List<object>();
            
            foreach (var strategy in _strategies) {
                if (!strategy.Key.Resolves(envelope.Message)) {
                    continue;
                }
                
                var completion = new TaskCompletionSource<object>();
                
                await strategy.Value(envelope
                    .WithOnSuccess(result => {
                        results.Add(result);
                        completion.TrySetResult(result);
                    })
                    .WithOnError(error => {
                        completion.TrySetException(error);
                    })
                );

                tasks.Add(completion.Task);
                
                if (tasks.Count >= _maxNumberOfMatches) {
                    break;
                }
            }

            if (1 == tasks.Count) {
                try {
                    await tasks.First();
                    envelope.OnSuccess(results.First());
                } catch (Exception error) {
                    envelope.OnError(error);
                }
                return;
            }

            if (0 != tasks.Count) {
                try {
                    await Task.WhenAll(tasks);
                    envelope.OnSuccess(results.ToArray());
                } catch (Exception error) {
                    envelope.OnError(error);
                }
                return;
            }
            
            var handlerList = string.Join(", ", _strategies.Select(s => s.Key.GetType().GenericTypeArguments.First().Name));
            throw new UnroutableMessageException($"Message ({envelope.MessageName}) not handled by ({handlerList})");
        }
    }
}
