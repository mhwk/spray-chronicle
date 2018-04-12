using System;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryHandler<T> : QueryExecutor<T>, IHandleQueries where T : QueryHandler<T>
    {
        private readonly IMessageHandlingStrategy<T> _projectors;

        protected QueryHandler()
            : this(
                new OverloadHandlingStrategy<T>("Execute"),
                new OverloadHandlingStrategy<T>("Process")
            )
        {
        }
        
        protected QueryHandler(
            IMessageHandlingStrategy<T> executors,
            IMessageHandlingStrategy<T> projectors)
            : base(executors)
        {
            _projectors = projectors;
        }

        public async Task Process(object @event, DateTime at)
        {
            await _projectors.Tell(this as T, @event.ToMessage(), at);
        }
    }
}
