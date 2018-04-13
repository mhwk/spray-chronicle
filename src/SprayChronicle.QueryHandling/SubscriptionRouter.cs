using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public sealed class SubscriptionRouter : IQueryRouter
    {
        public delegate Task<QueryMetadata> Execute(object query);
        
        private readonly Dictionary<Type,List<Execute>> _executors = new Dictionary<Type,List<Execute>>();

        public async Task<QueryMetadata[]> Route(object query)
        {
            if (!_executors.ContainsKey(query.GetType())) {
                throw new UnhandledQueryException(string.Format(
                    "Query {0} not handled by one of the executors {1}",
                    query.GetType(),
                    string.Join(", ", _executors.Select(p => p.GetType().ToString()).ToArray())
                ));
            }

            return await Task.WhenAll(
                _executors[query.GetType()]
                    .Select(e => e.GetMethodInfo().Invoke(e, new [] { query }) as Task<QueryMetadata>)
            );
        }

        public void Subscribe(IQueryExecutor executor)
        {
            
        }

        public void Subscribe<T>(Execute executor)
        {
            if (!_executors.ContainsKey(typeof(T))) {
                _executors.Add(typeof(T), new List<Execute>());
            }
            
            _executors[typeof(T)].Add(executor);
        }
    }
}
