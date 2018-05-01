using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SprayChronicle.EventHandling;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public abstract class EventSourced<T> : IEventSourcable<T> where T : EventSourced<T>
    {
        private long _sequence = -1;

        private readonly List<Tuple<long,object,DateTime>> _events = new List<Tuple<long,object,DateTime>>();

        private static readonly IMailStrategy<T> Strategy = new OverloadMailStrategy<T>();

        public abstract string Identity();

        public IEnumerable<Tuple<long,object,DateTime>> Diff()
        {
            var diff = _events.ToArray();
            _events.Clear();
            return diff;
        }

        public static async Task<T> Patch(IEventSource<T> messages)
        {
            var sourcable = default(T);
            var sequence = -1L;

            var converted = new TransformBlock<object, DomainEnvelope>(
                message =>
                {
                    try {
                        return messages.Convert(Strategy, message);
                    } catch (UnsupportedMessageException) {
                        return null;
                    }
                });
            var applied = new ActionBlock<DomainEnvelope>(async message => {
                sequence++;
                
                if (null != message) {
                    sourcable = await Strategy.Ask<T>(sourcable, message.Message, message.Epoch);
                }

                if (null != sourcable) {
                    sourcable._sequence = sequence;
                }
            });

            messages.LinkTo(converted, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            converted.LinkTo(applied, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await messages.Start();
            await messages.Completion;
            await converted.Completion;
            await applied.Completion;
            
            return sourcable;
        }

        protected static async Task<T> Apply(EventSourced<T> sourcable, object message)
        {
            var epoch = DateTime.Now;
            var updated = await Strategy.Ask<EventSourced<T>>(sourcable as T, message, epoch);
            
            if (updated != sourcable && null != sourcable) {
                updated._events.AddRange(sourcable._events);
            }

            if (null == updated) return null;
            
            updated._sequence = sourcable?._sequence + 1 ?? 0;
            updated._events.Add(new Tuple<long,object,DateTime>(updated._sequence, message, epoch));

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
            var sourcable = default(T);
            foreach (var payload in payloads) {
                sourcable = await Apply(sourcable, payload);
            }
            return sourcable;
        }
    }
}
