using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public abstract class EventSourced<T> : IEventSourcable<T> where T : EventSourced<T>
    {
        private long _sequence = -1;

        private readonly List<IDomainMessage> _queue = new List<IDomainMessage>();

        private static readonly IMessagingStrategy<T> Strategy = new OverloadMessagingStrategy<T>();

        public abstract string Identity();

        public IEnumerable<IDomainMessage> Diff()
        {
            var diff = _queue.ToArray();
            _queue.Clear();
            return diff;
        }

        public static async Task<T> Patch(IEventSource<T> messages)
        {
            var sourcable = default(T);
            
            var converted = new TransformBlock<object, DomainMessage>(
                message =>
                {
                    try {
                        return messages.Convert(Strategy, message);
                    } catch (UnsupportedMessageException) {
                        return null;
                    }
                });
            var applied = new ActionBlock<DomainMessage>(async message => {
                if (null != message) {
                    sourcable = await Strategy.Ask<T>(sourcable, message.Payload, message.Epoch);
                }

                if (null != sourcable) {
                    sourcable._sequence++;
                }
            });

            messages.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(applied, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await messages.Start();
            await applied.Completion;
            
            return sourcable;
        }

        protected static async Task<T> Apply(IEventSourcable<T> sourcable, object payload)
        {
            var domainMessage = new DomainMessage(
                ((EventSourced<T>) sourcable)?._sequence + 1 ?? 0,
                new DateTime(),
                payload
            );

            var updated = (IEventSourcable<T>) await Strategy.Ask<EventSourced<T>>(sourcable as T, domainMessage.Payload, domainMessage.Epoch);
            
            if (updated != sourcable && null != sourcable) {
                ((EventSourced<T>)updated)._queue.AddRange(((EventSourced<T>)sourcable)._queue);
            }

            ((EventSourced<T>)updated)._sequence = domainMessage.Sequence;
            ((EventSourced<T>)updated)._queue.Add(domainMessage);

            return (T) updated;
        }

        protected static async Task<T> Apply(IEventSourcable<T> sourcable, params object[] payloads)
        {
            foreach (var payload in payloads) {
                sourcable = await Apply(sourcable, payload);
            }
            return (T) sourcable;
        }

        protected static async Task<T> Apply(object payload)
        {
            return await Apply(null, payload);
        }

        protected static async Task<T> Apply(params object[] payloads)
        {
            T sourcable = default(T);
            foreach (var payload in payloads) {
                sourcable = await Apply(sourcable, payload);
            }
            return sourcable;
        }
    }
}
