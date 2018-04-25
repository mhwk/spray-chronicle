using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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

        private readonly OuroSourceFactory _sourceFactory;
        
        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        public OuroEventStore(
            ILogger<IEventStore> logger,
            OuroSourceFactory sourceFactory,
            IEventStoreConnection eventStore,
            UserCredentials credentials)
        {
            _logger = logger;
            _sourceFactory = sourceFactory;
            _eventStore = eventStore;
            _credentials = credentials;
        }

        public async Task Append<T>(string identity, IEnumerable<IDomainMessage> domainMessages)
        {
            var enumerable = domainMessages as DomainMessage[] ?? domainMessages.ToArray();
            
            if ( ! enumerable.Any()) {
                return;
            }
            
            try {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await _eventStore.AppendToStreamAsync(
                    Stream<T>(identity),
                    enumerable.First().Sequence - 1,
                    enumerable.Select(BuildEventData),
                    _credentials
                );

                stopwatch.Stop();
                _logger.LogDebug($"[{Stream<T>(identity)}::append] {stopwatch.ElapsedMilliseconds}ms");
            } catch (AggregateException error) {
                throw new ConcurrencyException(string.Format(
                    $"Concurrency detected: {error.InnerException?.Message}"
                ));
            }
        }

        public IEventSource<T> Load<T>(string identity)
            where T : class
        {
            return _sourceFactory.Build<T,ReadForwardOptions>(new ReadForwardOptions(Stream<T>(identity)));
        }

        private static string Stream<T>(string identity)
        {
            if (identity.Equals("")) {
                throw new InvalidStreamException("Stream name can not be empty");
            }
            if (identity.Contains("@")) {
                throw new InvalidStreamException($"Stream name {identity} contains invalid character '@'");
            }

            return $"{typeof(T).Namespace?.Split('.').First()}-{identity}";
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
