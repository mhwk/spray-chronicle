using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.MessageHandling
{
    public abstract class OneToOneRouter<THandler> : IRouter<THandler>
        where THandler : class
    {
        private readonly Dictionary<IMessageHandlingStrategy<THandler>,HandleMessage> _strategies = new Dictionary<IMessageHandlingStrategy<THandler>,HandleMessage>();

        public IRouter<THandler> Subscribe(IMessageHandlingStrategy<THandler> strategy, HandleMessage handler)
        {
            if (_strategies.ContainsKey(strategy)) {
                throw new Exception($"Strategy {strategy.GetType()} already subscribed");
            }
            
            _strategies.Add(strategy, handler);

            return this;
        }

        public IRouter<THandler> Subscribe(IRouterSubscriber<THandler> subscriber)
        {
            subscriber.Subscribe(this);
            return this;
        }
        
        public async Task<object> Route(params object[] arguments)
        {
            var tasks = new List<Task<object>>();
            
            foreach (var strategy in _strategies) {
                if (!strategy.Key.Resolves(strategy.Value, arguments)) {
                    continue;
                }

                tasks.Add(strategy.Value(arguments));
            }

            if (0 != tasks.Count) return await Task.WhenAll(tasks);
            
            var messageList = string.Join(", ", arguments.Select(m => m.GetType().Name));
            var handlerList = string.Join(", ", _strategies.Select(s => s.Value.GetType().Name));
            throw new UnhandledMessageException($"Messages ({messageList}) not handled by ({handlerList})");
        }
    }
}
