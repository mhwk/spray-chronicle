using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprayChronicle.QueryHandling
{
    public class SubscriptionQueryProcessor : IProcessQueries
    {
        private readonly List<IExecuteQueries> _executors = new List<IExecuteQueries>();

        public void AddExecutors(params IExecuteQueries[] executors)
        {
            _executors.AddRange(executors);
        }

        public void AddExecutors(IEnumerable<IExecuteQueries> executors)
        {
            _executors.AddRange(executors);
        }

        public async Task<object> Process(object query)
        {
            return await Task.Run(() => {
                var processor = _executors.FirstOrDefault(p => p.Executes(query));
            
                if (null == processor) {
                    throw new UnhandledQueryException(string.Format(
                        "Query {0} not handled by one of the executors {1}",
                        query.GetType(),
                        string.Join(", ", _executors.Select(p => p.GetType().ToString()).ToArray())
                    ));
                }

                return processor.Execute(query);
            });
        }
    }
}
