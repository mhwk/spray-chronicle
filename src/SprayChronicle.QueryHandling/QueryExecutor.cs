using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryExecutor<T> : IExecuteQueries where T : QueryExecutor<T>
    {
        private readonly IMessageHandlingStrategy<T> _executors;

        protected QueryExecutor() : this(new OverloadHandlingStrategy<T>("Execute"))
        {
        }

        protected QueryExecutor(IMessageHandlingStrategy<T> executors)
        {
            _executors = executors;
        }

        public async Task<object> Execute(object query)
        {
            return await _executors.Ask<object>(this as T, query.ToMessage());
        }
    }
}
