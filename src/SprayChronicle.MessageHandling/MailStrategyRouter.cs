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
        
        public async Task<object> Route(IEnvelope envelope)
        {
            var tasks = new List<Task<object>>();
            
            foreach (var strategy in _strategies) {
                if (!strategy.Key.Resolves(envelope.Message)) {
                    continue;
                }

                tasks.Add(strategy.Value(envelope));

                if (tasks.Count >= _maxNumberOfMatches) {
                    break;
                }
            }

            if (1 == tasks.Count) {
                return await tasks.First();
            }
            
            if (0 != tasks.Count) return await Task.WhenAll(tasks);
            
            var handlerList = string.Join(", ", _strategies.Select(s => s.Key.GetType().GenericTypeArguments.First().Name));
            throw new UnroutableMessageException($"Message ({envelope.MessageName}) not handled by ({handlerList})");
        }
    }
}
