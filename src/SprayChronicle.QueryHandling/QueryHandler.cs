using System;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryHandler<T> : QueryExecutor<T>, IHandleQueries
        where T : class
    {
        private readonly IMessageHandlingStrategy _projectors;

        protected QueryHandler(IStatefulRepository<T> repository)
            : this(
                repository,
                new OverloadHandlingStrategy<QueryHandler<T>>(new ContextTypeLocator<T>(), "Execute"),
                new OverloadHandlingStrategy<QueryHandler<T>>(new ContextTypeLocator<T>(), "Process")
            )
        {
        }
        
        protected QueryHandler(
            IStatefulRepository<T> repository,
            IMessageHandlingStrategy executors,
            IMessageHandlingStrategy projectors)
            : base(repository, executors)
        {
            _projectors = projectors;
        }

        public bool Processes(object @event, DateTime at)
        {
            return _projectors.AcceptsMessage(this, @event.ToMessage(), at);
        }

        public void Process(object @event, DateTime at)
        {
            _projectors.ProcessMessage(this, @event.ToMessage(), at);
        }
    }
}
