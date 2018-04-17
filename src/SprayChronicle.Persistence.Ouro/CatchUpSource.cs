using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using SprayChronicle.EventSourcing;
using SprayChronicle.Server;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class CatchUpSource<TSourceTarget> : OuroSource<TSourceTarget>
        where TSourceTarget : class
    {
        private readonly ILogger<IEventStore> _logger;
        
        private readonly IEventStoreConnection _eventStore;
        
        private readonly UserCredentials _credentials;

        private readonly string _streamName;

        private EventStoreCatchUpSubscription _subscription;
        
        private Exception _error;
        
        private bool _liveProcessing;

        private long _checkPoint;

        public CatchUpSource(
            ILogger<IEventStore> logger,
            IEventStoreConnection eventStore,
            UserCredentials credentials,
            string streamName)
        {
            _logger = logger;
            _eventStore = eventStore;
            _credentials = credentials;
            _streamName = streamName;
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
            _logger.LogDebug($"Start subscription {_streamName}");
            return _eventStore.SubscribeToStreamFrom(
                _streamName,
                _checkPoint,
                CatchUpSubscriptionSettings.Default,
                EventAppeared,
                LiveProcessingStarted,
                SubscriptionDropped,
                _credentials
            );
        }

        private Task EventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            _checkPoint++;
            _domainMessages.Post(resolvedEvent);
            
            _logger.LogDebug($"Emit subscription  {_streamName} - {resolvedEvent.Event.EventType}");

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
                    _logger.LogError(error, $"Errored subscription {_streamName}");
                    _error = error;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }
    }
}
