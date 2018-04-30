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

        private readonly StreamOptions _streamOptions;
        
        private readonly Func<StreamOptions, Task> _initializeStream;

        private readonly string _groupName;

        private EventStorePersistentSubscriptionBase _subscription;

        public PersistentSource(
            ILogger<TTarget> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            PersistentOptions options,
            Func<StreamOptions,Task> initializeStream) : base(logger, options.CausationId)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamOptions = options.StreamOptions;
            _initializeStream = initializeStream;
            _groupName = options.GroupName;
        }

        protected override async Task StartBuffering()
        {
            _initializeStream(_streamOptions);
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
                    _streamOptions.TargetStream,
                    _groupName,
                    PersistentSubscriptionSettings.Create()
                        .ResolveLinkTos()
                        .StartFromBeginning()
                        .Build(),
                    _credentials
                );
                _logger.LogDebug($"Created subscription {_streamOptions}_{_groupName}");
            } catch (AggregateException) {
                _logger.LogDebug($"Continuing subscription {_streamOptions}_{_groupName}");
            }

            return _eventStore.ConnectToPersistentSubscription(
                _streamOptions.TargetStream,
                _groupName,
                (subscription, resolvedEvent) => {
                    Queue.Post(resolvedEvent);
                    // Handle ack / nak flow through tpl?
                    // Or do we catch-up all the things?
                },
                (subscription, reason, error) => {
                    _logger.LogCritical(error, $"Errored persistent subscription {_streamOptions}_{_groupName}: {reason}, {error}");
                },
                _credentials
            );
        }
    }
}
