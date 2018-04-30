using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class CatchUpSource<TTarget> : OuroSource<TTarget>
        where TTarget : class
    {
        private readonly ILogger<TTarget> _logger;
        
        private readonly IEventStoreConnection _eventStore;
        
        private readonly UserCredentials _credentials;
        private readonly Func<StreamOptions, Task> _initializeStream;

        private readonly StreamOptions _streamOptions;
        
        private long _checkpoint;

        private EventStoreCatchUpSubscription _subscription;
        
        private Exception _error;
        
        private bool _liveProcessing;

        public CatchUpSource(
            ILogger<TTarget> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            CatchUpOptions options,
            Func<StreamOptions,Task> initializeStream) : base(logger, options.CausationId)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _initializeStream = initializeStream;
            _streamOptions = options.StreamOptions;
            _checkpoint = options.Checkpoint;
        }

        protected override async Task StartBuffering()
        {
            await _initializeStream(_streamOptions);
            _subscription = Subscribe();
        }

        protected override Task StopBuffering()
        {
            _subscription.Stop();
            return Task.CompletedTask;
        }

        private EventStoreCatchUpSubscription Subscribe()
        {
            _logger.LogDebug($"Start subscription {_streamOptions} from {_checkpoint}");
            if (-1 == _checkpoint) {
                return _eventStore.SubscribeToStreamFrom(
                    _streamOptions.TargetStream,
                    null,
                    new CatchUpSubscriptionSettings(10000, 500, false, true, Guid.NewGuid().ToString()),
                    EventAppeared,
                    LiveProcessingStarted,
                    SubscriptionDropped,
                    _credentials
                );
            }
            return _eventStore.SubscribeToStreamFrom(
                _streamOptions.TargetStream,
                _checkpoint,
                new CatchUpSubscriptionSettings(10000, 500, false, true, Guid.NewGuid().ToString()),
                EventAppeared,
                LiveProcessingStarted,
                SubscriptionDropped,
                _credentials
            );
        }

        private async Task EventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            _checkpoint++;
            try {
                await Queue.SendAsync(resolvedEvent);
            } catch (Exception error) {
                _logger.LogCritical(error);
                throw;
            }
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {
            _logger.LogDebug($"Live subscription {_streamOptions} after {_checkpoint} messages");
            _liveProcessing = true;
        }

        private void SubscriptionDropped(
            EventStoreCatchUpSubscription subscription,
            SubscriptionDropReason reason,
            Exception error)
        {
            _liveProcessing = false;
            
            switch (reason) {
                case SubscriptionDropReason.UserInitiated:
                    _logger.LogDebug($"Dropped subscription {_streamOptions}");
                    break;
                case SubscriptionDropReason.ProcessingQueueOverflow:
                    _logger.LogWarning(error, $"Overflow subscription {_streamOptions}");
                    break;
                case SubscriptionDropReason.NotAuthenticated:
                case SubscriptionDropReason.AccessDenied:
                case SubscriptionDropReason.SubscribingError:
                case SubscriptionDropReason.ServerError:
                case SubscriptionDropReason.ConnectionClosed:
                case SubscriptionDropReason.CatchUpError:
                case SubscriptionDropReason.EventHandlerException:
                case SubscriptionDropReason.MaxSubscribersReached:
                case SubscriptionDropReason.PersistentSubscriptionDeleted:
                case SubscriptionDropReason.Unknown:
                case SubscriptionDropReason.NotFound:
                    _logger.LogCritical(error, $"Errored subscription {_streamOptions} ({reason}): {_error}");
                    _error = error;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }
    }
}
