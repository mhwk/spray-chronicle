using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Newtonsoft.Json;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroEventStore : IEventStore
    {
        readonly IEventStoreConnection _eventStore;

        public OuroEventStore(IEventStoreConnection eventStore)
        {
            _eventStore = eventStore;
        }

        public void Append<T>(string identity, IEnumerable<DomainMessage> domainMessages)
        {
            try {
                _eventStore.AppendToStreamAsync(
                    Stream<T>(identity),
                    domainMessages.First().Sequence - 1,
                    domainMessages.Select(dm => BuildEventData(dm))
                ).Wait();
            } catch (AggregateException error) {
                throw new ConcurrencyException(string.Format(
                    "Concurrency detected: {0}",
                    error.InnerException.Message
                ));
            }
        }

        public IEnumerable<DomainMessage> Load<T>(string identity)
        {
            bool eos = false;
            int current = 0;

            do {
                var slice = _eventStore.ReadStreamEventsForwardAsync(Stream<T>(identity), current, 50, false).Result;
                foreach (DomainMessage domainMessage in slice.Events.Select(ev => BuildDomainMessage(ev))) {
                    yield return domainMessage;
                    current++;
                }
                eos = slice.IsEndOfStream;
            } while (!eos);
        }

        string Stream<T>(string identity)
        {
            return string.Format(
                "{0}-{1}",
                typeof(T).Namespace.Split('.').First(),
                identity
            );
        }

        EventData BuildEventData(DomainMessage domainMessage)
        {
            return new EventData(
                Guid.NewGuid(),
                domainMessage.Payload.GetType().Name,
                true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(domainMessage.Payload)),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Metadata(domainMessage.Payload.GetType())))
            );
        }

        DomainMessage BuildDomainMessage(ResolvedEvent resolvedEvent)
        {
            var metadata = JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata));

            return new DomainMessage(
                resolvedEvent.Event.EventNumber,
                resolvedEvent.Event.Created,
                JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                    Type.GetType(metadata.OriginalFqn)
                )
            );
        }

        public class Metadata
        {
            public readonly string OriginalFqn;

            public Metadata(Type originalFqn)
            {
                OriginalFqn = string.Format(
                    "{0}, {1}",
                    originalFqn.ToString(),
                    originalFqn.GetTypeInfo().Assembly
                );
            }
        }
    }
}
