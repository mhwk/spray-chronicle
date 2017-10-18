using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroEventStore : IEventStore
    {
        readonly ILogger<IEventStore> _logger;

        readonly IEventStoreConnection _eventStore;

        readonly UserCredentials _credentials;

        readonly string _tenant;

        public OuroEventStore(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            string tenant)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _tenant = tenant;
        }

        public void Append<T>(string identity, IEnumerable<DomainMessage> domainMessages)
        {
            if (0 == domainMessages.Count()) {
                return;
            }
            
            try {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                _eventStore.AppendToStreamAsync(
                    Stream<T>(identity),
                    domainMessages.First().Sequence - 1,
                    domainMessages.Select(dm => BuildEventData(dm)),
                    _credentials
                ).Wait();

                stopwatch.Stop();
                _logger.LogDebug("[{0}::append] {1}ms", Stream<T>(identity), stopwatch.ElapsedMilliseconds);
            } catch (AggregateException error) {
                throw new ConcurrencyException(string.Format(
                    "Concurrency detected: {0}",
                    error.InnerException.Message
                ));
            }
        }

        public IEnumerable<DomainMessage> Load<T>(string identity)
        {
            var stream = Stream<T>(identity);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            bool eos = false;
            int current = 0;

            do {
                var slice = _eventStore.ReadStreamEventsForwardAsync(stream, current, 50, false, _credentials).Result;
                foreach (var resolvedEvent in slice.Events) {
                    var metadata = JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(resolvedEvent.Event.Metadata));

                    if (metadata.Tenant == _tenant) {
                        yield return BuildDomainMessage(metadata, resolvedEvent);
                    }

                    current++;
                }
                eos = slice.IsEndOfStream;
            } while (!eos);

            stopwatch.Stop();
            _logger.LogDebug("[{0}::load] {1}ms", stream, stopwatch.ElapsedMilliseconds);
        }

        string Stream<T>(string identity)
        {
            if (identity.Equals("")) {
                throw new InvalidStreamException(string.Format(
                    "Stream name can not be empty",
                    identity
                ));
            }
            if (identity.Contains("@")) {
                throw new InvalidStreamException(string.Format(
                    "Stream name {0} contains invalid character '@'",
                    identity
                ));
            }
            return string.Format(
                "{0}-{1}-{2}",
                typeof(T).Namespace.Split('.').First(),
                _tenant,
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
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Metadata(
                    domainMessage.Payload.GetType(),
                    _tenant
                )))
            );
        }

        DomainMessage BuildDomainMessage(Metadata metadata, ResolvedEvent resolvedEvent)
        {
            return new DomainMessage(
                resolvedEvent.Event.EventNumber,
                resolvedEvent.Event.Created,
                JsonConvert.DeserializeObject(
                    Encoding.UTF8.GetString(resolvedEvent.Event.Data),
                    Type.GetType(metadata.OriginalFqn)
                )
            );
        }
    }
}
