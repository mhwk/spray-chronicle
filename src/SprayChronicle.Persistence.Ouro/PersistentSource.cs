using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class PersistentSource<TTarget> : OuroSource<TTarget>
        where TTarget : class
    {
        private readonly ILogger<TTarget> _logger;

        private readonly IEventStoreConnection _eventStore;

        private readonly UserCredentials _credentials;

        private readonly string _streamName;

        private readonly string _groupName;

        private EventStorePersistentSubscriptionBase _subscription;

        public PersistentSource(
            ILogger<TTarget> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            PersistentOptions options)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = options.StreamName;
            _groupName = options.GroupName;
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
                _logger.LogDebug($"Created subscription {_streamName}_{_groupName}");
            } catch (AggregateException) {
                _logger.LogDebug($"Continuing subscription {_streamName}_{_groupName}");
            }

            return _eventStore.ConnectToPersistentSubscription(
                _streamName,
                _groupName,
                (subscription, resolvedEvent) => {
                    Queue.Post(resolvedEvent);
                    // Handle ack / nak flow through tpl?
                    // Or do we catch-up all the things?
                },
                (subscription, reason, error) => {
                    _logger.LogCritical(error, $"Errored persistent subscription {_streamName}_{_groupName}: {reason}, {error}");
                },
                _credentials
            );
        }
    }
}
