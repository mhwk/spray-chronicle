using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public class OverloadQueryExecutor<T> : IExecuteQueries
    {
        protected readonly IStatefulRepository<T> _repository;

        readonly static IMessageHandlingStrategy _handlers = new OverloadHandlingStrategy<T>();

        public OverloadQueryExecutor(IStatefulRepository<T> repository)
        {
            _repository = repository;
        }

        public bool Executes(object query)
        {
            return _handlers.AcceptsMessage(this, query);
        }

        public object Execute(object query)
        {
            return _handlers.ProcessMessage(this, query);
        }
    }
}
