using System.Linq;
using System.Collections.Generic;

namespace SprayChronicle.QueryHandling
{
    public class SubscriptionQueryExecutor : IExecuteQueries
    {
        readonly List<IProcessQueries> _processors = new List<IProcessQueries>();

        public void AddProcessors(params IProcessQueries[] processor)
        {
            _processors.AddRange(processor);
        }

        public void AddProcessors(IEnumerable<IProcessQueries> processors)
        {
            _processors.AddRange(processors);
        }

        public object Execute(object query)
        {
            var processor = _processors
                .Where(p => p.Processes(query))
                .FirstOrDefault();
            
            if (null == processor) {
                throw new UnhandledQueryException(string.Format(
                    "Query {0} not handler by one of the processors {1}",
                    query.GetType(),
                    string.Join(", ", _processors.Select(p => p.GetType().ToString()).ToArray())
                ));
            }

            return processor.Process(query);
        }
    }
}
