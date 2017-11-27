using System;
using System.Collections.Generic;
using System.Linq;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public abstract class EventSourced<T> : IEventSourcable<T> where T : IEventSourcable<T>
    {
        private long _sequence = -1;

        private readonly List<DomainMessage> _queue = new List<DomainMessage>();

        private static readonly IMessageHandlingStrategy Router = new OverloadHandlingStrategy<T>(new ContextTypeLocator<T>());

        public abstract string Identity();

        public IEnumerable<DomainMessage> Diff()
        {
            var diff = _queue.ToArray();
            _queue.Clear();
            return diff;
        }

        public static T Patch(IEnumerable<DomainMessage> messages)
        {
            var sourcable = default(IEventSourcable<T>);
            foreach (var message in messages) {
                if (Router.AcceptsMessage(sourcable, message.Payload)) {
                    sourcable = (EventSourced<T>) Router.ProcessMessage(sourcable, message.Payload);
                }
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
                new InstanceMessage(payload)
            );

            var updated = sourcable;
            if (Router.AcceptsMessage(sourcable, domainMessage.Payload)) {
                updated = (IEventSourcable<T>) Router.ProcessMessage(sourcable, domainMessage.Payload);
            }
            
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
