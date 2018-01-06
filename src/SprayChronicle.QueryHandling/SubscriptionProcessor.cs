using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var processor = _executors.FirstOrDefault(p => p.Executes(query));
            
            if (null == processor) {
                throw new UnhandledQueryException(string.Format(
                    "Query {0} not handled by one of the executors {1}",
                    query.GetType(),
                    string.Join(", ", _executors.Select(p => p.GetType().ToString()).ToArray())
                ));
            }
            
            return await Task.Run(() => processor.Execute(query));
        }
    }
}
