using System;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryHandler<T> : IExecuteQueries, IHandleEvents
        where T : class
    {
        private readonly IStatefulRepository<T> _repository;
        
        private readonly IMessageHandlingStrategy _queryExecutors;
        
        private readonly IMessageHandlingStrategy _eventProjectors;

        protected QueryHandler(IStatefulRepository<T> repository)
            : this(
                repository,
                new OverloadHandlingStrategy<QueryHandler<T>>(new ContextTypeLocator<T>(), "Execute"),
                new OverloadHandlingStrategy<QueryHandler<T>>(new ContextTypeLocator<T>(), "Process")
            )
        {
        }
        
        protected QueryHandler(IStatefulRepository<T> repository, IMessageHandlingStrategy queryExecutors, IMessageHandlingStrategy eventProjectors)
        {
            _repository = repository;
            _queryExecutors = queryExecutors;
            _eventProjectors = eventProjectors;
        }

        protected IStatefulRepository<T> Repository()
        {
            return _repository;
        }

        public bool Executes(object query)
        {
            return _queryExecutors.AcceptsMessage(this, query.ToMessage());
        }

        public object Execute(object query)
        {
            return _queryExecutors.ProcessMessage(this, query.ToMessage());
        }

        public bool Processes(object @event, DateTime at)
        {
            return _eventProjectors.AcceptsMessage(this, @event.ToMessage(), at);
        }

        public void Process(object @event, DateTime at)
        {
            _eventProjectors.ProcessMessage(this, @event.ToMessage(), at);
        }
    }
}
