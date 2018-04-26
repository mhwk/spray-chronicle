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

        private readonly string _streamName;
        
        private long _checkpoint;

        private EventStoreCatchUpSubscription _subscription;
        
        private Exception _error;
        
        private bool _liveProcessing;

        public CatchUpSource(
            ILogger<TTarget> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            CatchUpOptions options) : base(logger)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = options.StreamName;
            _checkpoint = options.Checkpoint;
        }

        protected override Task StartBuffering()
        {
            _subscription = Subscribe();
            return Task.CompletedTask;
        }

        protected override Task StopBuffering()
        {
            _subscription.Stop();
            return Task.CompletedTask;
        }

        private EventStoreCatchUpSubscription Subscribe()
        {
            _logger.LogDebug($"Start subscription {_streamName} from {_checkpoint}");
            return _eventStore.SubscribeToStreamFrom(
                _streamName,
                _checkpoint,
                CatchUpSubscriptionSettings.Default,
                EventAppeared,
                LiveProcessingStarted,
                SubscriptionDropped,
                _credentials
            );
        }

        private Task EventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            _checkpoint++;
            
            Queue.Post(resolvedEvent);
            
            return Task.CompletedTask;
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription subscription)
        {
            Console.WriteLine($"Live subscription {_streamName}");
            
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
                    _logger.LogDebug($"Dropped subscription {_streamName}");
                    break;
                case SubscriptionDropReason.ProcessingQueueOverflow:
                    _logger.LogDebug($"Overflow subscription {_streamName}");
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
                    _logger.LogCritical(error, $"Errored subscription {_streamName} ({reason}): {_error}");
                    _error = error;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }
    }
}
