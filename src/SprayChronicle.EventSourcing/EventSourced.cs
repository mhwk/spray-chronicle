using System;
using System.Collections.Generic;

namespace SprayChronicle.EventSourcing
{
    public abstract class EventSourced<T> : IEventSourcable<T> where T : IEventSourcable<T>
    {
        int _sequence = -1;

        List<DomainMessage> _queue = new List<DomainMessage>();

        static IEventRouter<T> _router = new OverloadRouter<T>();

        public abstract string Identity();

        public IEnumerable<DomainMessage> Diff()
        {
            DomainMessage[] diff = _queue.ToArray();
            _queue.Clear();
            return diff;
        }

        public static T Patch(IEnumerable<DomainMessage> messages)
        {
            IEventSourcable<T> sourcable = default(IEventSourcable<T>);
            foreach (var message in messages) {
                sourcable = (EventSourced<T>) _router.Route(sourcable, message);
                ((EventSourced<T>)sourcable)._sequence = message.Sequence;
            }
            return (T) sourcable;
        }

        protected static T Apply(IEventSourcable<T> sourcable, object payload)
        {
            var domainMessage = new DomainMessage(
                null == sourcable ? 0 : ((EventSourced<T>)sourcable)._sequence + 1,
                new DateTime(),
                payload
            );

            var updated = _router.Route(sourcable, domainMessage);
            
            if (updated != sourcable && null != sourcable) {
                ((EventSourced<T>)updated)._queue.AddRange(((EventSourced<T>)sourcable)._queue);
            }

            ((EventSourced<T>)updated)._sequence = domainMessage.Sequence;
            ((EventSourced<T>)updated)._queue.Add(domainMessage);

            return (T) updated;
        }
    }
}
