using System.Linq;
using System.Collections.Generic;

namespace SprayChronicle.QueryHandling
{
    public class SubscriptionQueryProcessor : IProcessQueries
    {
        readonly List<IExecuteQueries> _executors = new List<IExecuteQueries>();

        public void AddExecutors(params IExecuteQueries[] executors)
        {
            _executors.AddRange(executors);
        }

        public void AddExecutors(IEnumerable<IExecuteQueries> executors)
        {
            _executors.AddRange(executors);
        }

        public object Process(object query)
        {
            var processor = _executors
                .Where(p => p.Executes(query))
                .FirstOrDefault();
            
            if (null == processor) {
                throw new UnhandledQueryException(string.Format(
                    "Query {0} not handled by one of the executors {1}",
                    query.GetType(),
                    string.Join(", ", _executors.Select(p => p.GetType().ToString()).ToArray())
                ));
            }

            return processor.Execute(query);
        }
    }
}
