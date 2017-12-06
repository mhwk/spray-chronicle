using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryExecutor<T> : IExecuteQueries where T : class
    {
        private readonly IStatefulRepository<T> _repository;

        private readonly IMessageHandlingStrategy _executors;

        protected QueryExecutor(IStatefulRepository<T> repository)
            : this(repository, new OverloadHandlingStrategy<QueryExecutor<T>>(new ContextTypeLocator<T>()))
        {
        }

        protected QueryExecutor(IStatefulRepository<T> repository, IMessageHandlingStrategy executors)
        {
            _repository = repository;
            _executors = executors;
        }

        protected IStatefulRepository<T> Repository()
        {
            return _repository;
        }

        public bool Executes(object query)
        {
            return _executors.AcceptsMessage(this, query.ToMessage());
        }

        public object Execute(object query)
        {
            return _executors.ProcessMessage(this, query.ToMessage());
        }
    }
}
