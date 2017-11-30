using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public class OverloadQueryExecutor<T> : IExecuteQueries
    {
        protected readonly IStatefulRepository<T> Repository;

        private static readonly IMessageHandlingStrategy Handlers = new OverloadHandlingStrategy<OverloadQueryExecutor<T>>(new ContextTypeLocator<T>());

        public OverloadQueryExecutor(IStatefulRepository<T> repository)
        {
            Repository = repository;
        }

        public bool Executes(object query)
        {
            return Handlers.AcceptsMessage(this, query.ToMessage());
        }

        public object Execute(object query)
        {
            return Handlers.ProcessMessage(this, query.ToMessage());
        }
    }
}
