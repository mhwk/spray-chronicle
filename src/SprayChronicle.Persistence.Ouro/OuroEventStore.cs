using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroEventStore : IEventStore, IEventSourceFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        
        private readonly IEventStoreConnection _connection;
        
        private readonly ProjectionsManager _projections;

        private readonly UserCredentials _credentials;

        public OuroEventStore(
            ILoggerFactory loggerFactory,
            IEventStoreConnection connection,
            ProjectionsManager projections,
            UserCredentials credentials)
        {
            _loggerFactory = loggerFactory;
            _connection = connection;
            _projections = projections;
            _credentials = credentials;
        }

        public async Task Append<T>(string identity, IEnumerable<IDomainEnvelope> domainMessages)
        {
            var enumerable = domainMessages as DomainEnvelope[] ?? domainMessages.ToArray();
            
            if ( ! enumerable.Any()) {
                return;
            }
            
            try {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await _connection.AppendToStreamAsync(
                    Stream<T>(identity),
                    enumerable.First().Sequence - 1,
                    enumerable.Select(BuildEventData),
                    _credentials
                );

                stopwatch.Stop();
            } catch (AggregateException error) {
                // @todo handle correct exception
                throw new ConcurrencyException(string.Format(
                    $"Concurrency detected: {error.InnerException?.Message}"
                ));
            }
        }

        public IEventSource<T> Load<T>(string identity, string causationId)
            where T : class
        {
            return Build<T,ReadForwardOptions>(
                new ReadForwardOptions(Stream<T>(identity))
                    .WithCausationId(causationId)
            );
        }

        private static string Stream<T>(string identity)
        {
            if (identity.Equals("")) {
                throw new InvalidStreamException("Stream name can not be empty");
            }
            if (identity.Contains("@")) {
                throw new InvalidStreamException($"Stream name {identity} contains invalid character '@'");
            }

            return $"{typeof(T).Name}-{identity}";
        }

        private EventData BuildEventData(IDomainEnvelope domainEnvelope)
        {
            return new EventData(
                Guid.NewGuid(),
                domainEnvelope.MessageName,
                true,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(domainEnvelope.Message)),
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Metadata(
                    domainEnvelope.MessageId,
                    domainEnvelope.CausationId,
                    domainEnvelope.CorrelationId
                )))
            );
        }

        public async Task EnsureProjection(StreamOptions streamOptions)
        {
            var query = streamOptions.BuildProjectionQuery();
            if (null == query) {
                return;
            }

            if (!(await ListProjections()).Select(p => p.Name).Contains(streamOptions.TargetStream)) {
                _loggerFactory.Create<OuroEventStore>().LogDebug(
                    $"Creating new projection {streamOptions.TargetStream}:\n{query}"
                );
                await _projections.CreateContinuousAsync(
                    streamOptions.TargetStream,
                    query,
                    false,
                    _credentials
                );
            } else {
                _loggerFactory.Create<OuroEventStore>().LogDebug(
                    $"Updating existing projection {streamOptions.TargetStream}:\n{query}"
                );
                await _projections.UpdateQueryAsync(
                    streamOptions.TargetStream,
                    query,
                    _credentials
                );
            }
        }

        public async Task<ProjectionRef[]> ListProjections()
        {
            var all = await _projections.ListAllAsync(_credentials);
            var projections = new List<ProjectionRef>();

            foreach (var projection in all) {
                projections.Add(new ProjectionRef(
                    projection.Name,
                    projection.Status
                ));
            }
            
            return projections.ToArray();
        }

        public IEventSource<TTarget> Build<TTarget, TOptions>(TOptions options)
            where TTarget : class
        {
            var readForwardOptions = options as ReadForwardOptions;
            if (null != readForwardOptions) {
                return new ReadForwardSource<TTarget>(
                    _loggerFactory.Create<TTarget>(),
                    _connection,
                    _credentials,
                    readForwardOptions,
                    EnsureProjection
                );
            }
            
            var catchUpOptions = options as CatchUpOptions;
            if (null != catchUpOptions) {
                return new CatchUpSource<TTarget>(
                    _loggerFactory.Create<TTarget>(),
                    _connection,
                    _credentials,
                    catchUpOptions,
                    EnsureProjection
                );
            }
            
            var persistentOptions = options as PersistentOptions;
            if (null != persistentOptions) {
                return new PersistentSource<TTarget>(
                    _loggerFactory.Create<TTarget>(),
                    _connection,
                    _credentials,
                    persistentOptions,
                    EnsureProjection
                );
            }
            

            throw new SourceBuildException($"SourceOptions {typeof(TOptions)} not supported");
        }
    }
}
