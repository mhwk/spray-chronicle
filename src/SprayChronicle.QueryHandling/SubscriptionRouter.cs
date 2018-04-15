using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public sealed class SubscriptionRouter : IQueryRouter
    {
        public delegate Task<object> Execute(object query);
        
        private readonly Dictionary<Type,List<Execute>> _executors = new Dictionary<Type,List<Execute>>();

        public async Task<object> Route(object query)
        {
            if (!_executors.ContainsKey(query.GetType())) {
                var executors = string.Join(", ", _executors.Select(kv => kv.Key.Name));
                throw new UnhandledQueryException($"Query {query.GetType()} not included in execution list ({executors})");
            }

            if (_executors[query.GetType()].Count == 1) {
                return _executors[query.GetType()]
                    .First()
                    .GetMethodInfo()
                    .Invoke(_executors[query.GetType()].First(), new[] {query}) as Task<object>;
            } 

            return await Task.WhenAll(
                _executors[query.GetType()]
                    .Select(e => e.GetMethodInfo().Invoke(e, new [] { query }) as Task<object>)
            );
        }

        public SubscriptionRouter Subscribe(IQueryRouterSubscriber subscriber)
        {
            subscriber.Subscribe(this);
            return this;
        }

        public void Subscribe(Type queryType, Execute executor)
        {
            if (!_executors.ContainsKey(queryType)) {
                _executors.Add(queryType, new List<Execute>());
            }
            
            _executors[queryType].Add(executor);
        }
    }
}
