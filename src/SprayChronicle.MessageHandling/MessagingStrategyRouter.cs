using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public abstract class MessagingStrategyRouter<THandler> : IMessageRouter, IMessagingStrategyRouter<THandler>
        where THandler : class
    {
        private readonly Dictionary<IMessagingStrategy,HandleMessage> _strategies = new Dictionary<IMessagingStrategy,HandleMessage>();

        public IMessagingStrategyRouter<THandler> Subscribe(IMessagingStrategy strategy, HandleMessage handler)
        {
            if (_strategies.ContainsKey(strategy)) {
                throw new Exception($"Strategy {strategy.GetType()} already subscribed");
            }
            
            _strategies.Add(strategy, handler);

            return this;
        }

        public IMessagingStrategyRouter<THandler> Subscribe(IMessagingStrategyRouterSubscriber<THandler> subscriber)
        {
            subscriber.Subscribe(this);
            return this;
        }
        
        public async Task<object> Route(params object[] arguments)
        {
            var tasks = new List<Task<object>>();
            
            foreach (var strategy in _strategies) {
                if (!strategy.Key.Resolves(arguments)) {
                    continue;
                }

                tasks.Add(strategy.Value(arguments));
                return await strategy.Value(arguments);
            }

            if (1 == tasks.Count) {
                return await tasks.First();
            }
            
            if (0 != tasks.Count) return await Task.WhenAll(tasks);
            
            var messageList = string.Join(", ", arguments.Select(m => m.GetType().Name));
            var handlerList = string.Join(", ", _strategies.Select(s => s.Key.GetType().GenericTypeArguments.First().Name));
            throw new UnroutableMessageException($"Message ({messageList}) not handled by ({handlerList})");
        }
    }
}
