using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class PersistentSource<TSourceTarget> : OuroSource<TSourceTarget>
        where TSourceTarget : class
    {
        private readonly ILogger<IEventStore> _logger;

        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        private readonly string _streamName;

        private readonly string _groupName;

        private EventStorePersistentSubscriptionBase _subscription;

        public PersistentSource(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            string streamName,
            string groupName)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = streamName;
            _groupName = groupName;
        }

        protected override async Task StartBuffering()
        {
            _subscription = await Subscribe();
        }

        protected override Task StopBuffering()
        {
            _subscription.Stop(TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private async Task<EventStorePersistentSubscriptionBase> Subscribe()
        {
            try {
                await _eventStore.CreatePersistentSubscriptionAsync(
                    _streamName,
                    _groupName,
                    PersistentSubscriptionSettings.Create()
                        .ResolveLinkTos()
                        .StartFromBeginning()
                        .Build(),
                    _credentials
                );
                _logger.LogDebug("Created subscription {0}_{1}", _streamName, _groupName);
            } catch (AggregateException) {
                _logger.LogDebug("Continuing subscription {0}_{1}", _streamName, _groupName);
            }

            return _eventStore.ConnectToPersistentSubscription(
                _streamName,
                _groupName,
                (subscription, resolvedEvent) => {
                    _domainMessages.Post(resolvedEvent);
                    // Handle ack / nak flow through tpl?
                    // Or do we catch-up all the things?
                },
                (subscription, reason, error) => {
                    _logger.LogCritical(error, $"Errored subscription {_streamName}_{_groupName}: {reason}, {error}");
                },
                _credentials
            );
        }
    }
}
