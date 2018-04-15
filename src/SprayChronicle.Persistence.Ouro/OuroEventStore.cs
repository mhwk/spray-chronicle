using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroEventStore : IEventStore
    {
        private readonly ILogger<IEventStore> _logger;

        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        public OuroEventStore(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
        }

        public void Append<T>(string identity, IEnumerable<IDomainMessage> domainMessages)
        {
            var enumerable = domainMessages as DomainMessage[] ?? domainMessages.ToArray();
            
            if ( ! enumerable.Any()) {
                return;
            }
            
            try {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                _eventStore.AppendToStreamAsync(
                    Stream<T>(identity),
                    enumerable.First().Sequence - 1,
                    enumerable.Select(BuildEventData),
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

        public IEnumerable<IDomainMessage> Load<T>(string identity)
        {
            var stream = Stream<T>(identity);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var eos = false;
            var current = 0;

            do {
                var slice = _eventStore.ReadStreamEventsForwardAsync(stream, current, 50, false, _credentials).Result;
                foreach (var resolvedEvent in slice.Events) {
                    yield return new OuroMessage(resolvedEvent);

                    current++;
                }
                eos = slice.IsEndOfStream;
            } while (!eos);

            stopwatch.Stop();
            _logger.LogDebug("[{0}::load] {1}ms", stream, stopwatch.ElapsedMilliseconds);
        }

        private static string Stream<T>(string identity)
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
                "{0}-{2}",
                typeof(T).Namespace.Split('.').First(),
                identity
            );
        }

        private EventData BuildEventData(IDomainMessage domainMessage)
        {
            return new EventData(
                Guid.NewGuid(),
                domainMessage.Name,
                true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(domainMessage.Payload)),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Metadata(
                    domainMessage.Payload.GetType()
                )))
            );
            
            
        }
    }
}
