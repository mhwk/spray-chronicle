using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public sealed class SubscriptionProcessor : IProcessQueries
    {
        private readonly List<IExecuteQueries> _executors = new List<IExecuteQueries>();

        public SubscriptionProcessor Subscribe(params IExecuteQueries[] executors)
        {
            _executors.AddRange(executors);

            return this;
        }

        public async Task<object> Process(object query)
        {
            var executor = _executors.FirstOrDefault(e => MessageHandlingMetadata.Accepts(e.GetType(), query.GetType()));
            
            if (null == executor) {
                throw new UnhandledQueryException(string.Format(
                    "Query {0} not handled by one of the executors {1}",
                    query.GetType(),
                    string.Join(", ", _executors.Select(p => p.GetType().ToString()).ToArray())
                ));
            }

            return await executor.Execute(query);
        }
    }
}
