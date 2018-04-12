using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public abstract class EventSourced<T> : IEventSourcable<T> where T : class, IEventSourcable<T>
    {
        private long _sequence = -1;

        private readonly List<IDomainMessage> _queue = new List<IDomainMessage>();

        private static readonly IMessageHandlingStrategy<T> Strategy = new OverloadHandlingStrategy<T>(new ContextTypeLocator<T>());

        public abstract string Identity();

        public IEnumerable<IDomainMessage> Diff()
        {
            var diff = _queue.ToArray();
            _queue.Clear();
            return diff;
        }

        public static async Task<T> Patch(IEnumerable<IDomainMessage> messages)
        {
            var sourcable = default(IEventSourcable<T>);
            foreach (var message in messages) {
                sourcable = await Strategy.Ask<EventSourced<T>>(sourcable as T, message);
                
                if (null == sourcable) {
                    continue;
                }
                
                ((EventSourced<T>)sourcable)._sequence = message.Sequence;
            }
            return (T) sourcable;
        }

        protected static T Apply(IEventSourcable<T> sourcable, object payload)
        {
            var domainMessage = new DomainMessage(
                ((EventSourced<T>) sourcable)?._sequence + 1 ?? 0,
                new DateTime(),
                payload
            );

            var updated = (IEventSourcable<T>) Strategy.Tell(sourcable as T, domainMessage);
            
            if (updated != sourcable && null != sourcable) {
                ((EventSourced<T>)updated)._queue.AddRange(((EventSourced<T>)sourcable)._queue);
            }

            ((EventSourced<T>)updated)._sequence = domainMessage.Sequence;
            ((EventSourced<T>)updated)._queue.Add(domainMessage);

            return (T) updated;
        }

        protected static T Apply(IEventSourcable<T> sourcable, params object[] payloads)
        {
            foreach (var payload in payloads) {
                sourcable = Apply(sourcable, payload);
            }
            return (T) sourcable;
        }

        protected static T Apply(object payload)
        {
            return Apply(null, payload);
        }

        protected static T Apply(params object[] payloads)
        {
            T sourcable = default(T);
            foreach (var payload in payloads) {
                sourcable = Apply(sourcable, payload);
            }
            return sourcable;
        }
    }
}
