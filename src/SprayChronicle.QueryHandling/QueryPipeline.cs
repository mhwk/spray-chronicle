using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.QueryHandling
{
    public abstract class QueryPipeline<TSession,TQueryProcessor> : IQueryPipeline
        where TQueryProcessor : class, IExecute, IProcess
    {
        public string Description => typeof(TQueryProcessor).ToString();
        
        private readonly IEventSource<DomainMessage> _events;
        
        private readonly IQueryQueue _queries;
        
        private readonly IMessageHandlingStrategy<TQueryProcessor> _eventStrategy = new OverloadHandlingStrategy<TQueryProcessor>("Process");
        
        private readonly IMessageHandlingStrategy<TQueryProcessor> _queryStrategy = new OverloadHandlingStrategy<TQueryProcessor>("Execute");
        
        private readonly TQueryProcessor _processor;

        public QueryPipeline(
            IEventSource<DomainMessage> events,
            IQueryQueue queries,
            TQueryProcessor processor)
        {
            _events = events;
            _queries = queries;
            _processor = processor;
        }

        public async Task Start()
        {
            await Task.WhenAll(
                RunEventProcessing(),
                RunQueryHandling()
            );
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }

        private async Task RunEventProcessing()
        {
            var eventsProcessed = new TransformBlock<DomainMessage,EventProcessed>((Func<DomainMessage, Task<EventProcessed>>) ProcessEvents);
            var eventsBatched = new BatchBlock<EventProcessed>(1000);
            var eventsApplied = new ActionBlock<EventProcessed[]>(ApplyEvents);
            
            _events.LinkTo(eventsProcessed);
            eventsProcessed.LinkTo(eventsBatched);
            eventsBatched.LinkTo(eventsApplied);
           
            await Task.WhenAll(
                _events.Start(),
                eventsApplied.Completion
            );
        }

        private async Task<EventProcessed> ProcessEvents(DomainMessage @event)
        {
            return (EventProcessed) ((dynamic) await _eventStrategy.Ask<EventProcessed>(_processor, @event.Payload, @event.Epoch).ConfigureAwait(false)).Result;
        }

        protected abstract Task ApplyEvents(EventProcessed[] processed);

        private async Task RunQueryHandling()
        {
            var queriesExecuted = new TransformBlock<QueryRequest,Tuple<Action<object>,QueryExecuted<TSession>>>(
                async request => new Tuple<Action<object>,QueryExecuted<TSession>>(
                    request.OnSuccess,
                    await ExecuteQuery(request)
                )
            );
            var queriesApplied = new TransformBlock<Tuple<Action<object>,QueryExecuted<TSession>>,Tuple<Action<object>,object>>(
                async tuple => new Tuple<Action<object>,object>(
                    tuple.Item1,
                    await ApplyQuery(tuple.Item2)
                )
            );
            var queriesSucceeded = new ActionBlock<Tuple<Action<object>,object>>(
                tuple => tuple.Item1(tuple.Item2)
            );

            _queries.LinkTo(queriesExecuted);
            queriesExecuted.LinkTo(queriesApplied);
            queriesApplied.LinkTo(queriesSucceeded);
            
            await Task.WhenAll(queriesSucceeded.Completion);
        }

        private async Task<QueryExecuted<TSession>> ExecuteQuery(QueryRequest query)
        {
            return (QueryExecuted<TSession>) ((dynamic) await _queryStrategy.Ask<QueryExecuted<TSession>>(_processor, query).ConfigureAwait(false)).Result;
        }

        protected abstract Task<object> ApplyQuery(QueryExecuted<TSession> executed);
        
        public void Subscribe(IRouter<IExecute> router)
        {
            router.Subscribe(_queryStrategy as IMessageHandlingStrategy<IExecute>, arguments =>
            {
                var onComplete = new TaskCompletionSource<object>();

                _queries.Post(new QueryRequest(
                    arguments,
                    result => onComplete.TrySetResult(result)
                ));

                return onComplete.Task;
            });
        }

        public void Subscribe(IRouter<IProcess> router)
        {
            router.Subscribe(_eventStrategy as IMessageHandlingStrategy<IProcess>, arguments =>
            {
                throw new NotImplementedException();
            });
        }
    }
}
