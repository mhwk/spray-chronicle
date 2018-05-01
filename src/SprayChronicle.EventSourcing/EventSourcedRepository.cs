using System;
using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.EventSourcing
{
    public sealed class EventSourcedRepository<T> : IEventSourcingRepository<T> where T : EventSourced<T>
    {
        private readonly IEventStore _persistence;

        public EventSourcedRepository(IEventStore persistence)
        {
            _persistence = persistence;
        }

        public async Task Save(T subject, IEnvelope envelope)
        {
            if (null == subject.Identity() || "" == subject.Identity()) {
                throw new UnknownStreamException("No identity found after apply of events");
            }
            
            await _persistence.Append<T>(subject.Identity(), subject.Diff().Select(m => new DomainEnvelope(
                Guid.NewGuid().ToString(),
                envelope.MessageId,
                envelope.CorrelationId,
                m.Item1,
                m.Item2,
                m.Item3
            )));
        }

        public async Task Save<TChild>(T sourced, IEnvelope envelope) where TChild : T
        {
            if (null == sourced) {
                throw new InvalidStateException(
                    $"Expected state {typeof(TChild)}, but got null"
                );
            }
            
            if ( ! (sourced is TChild)) {
                throw new InvalidStateException(
                    $"Expected state {typeof(TChild)}, but got {sourced.GetType()}"
                );
            }
            await Save(sourced, envelope);
        }

        public async Task<T> Load(string identity, string causationId)
        {
            return await EventSourced<T>.Patch(_persistence.Load<T>(identity, causationId));
        }

        public async Task<TChild> LoadOrDefault<TChild>(string identity, string idempotencyId) where TChild : T
        {
            var sourced = await Load(identity, idempotencyId);
            if (null == sourced) {
                return null;
            }
            if ( ! (sourced is TChild)) {
                throw new InvalidStateException(
                    $"Expected state {typeof(TChild)}, but got {sourced.GetType()}"
                );
            }
            return (TChild) sourced;
        }

        public async Task<TChild> Load<TChild>(string identity, string causationId) where TChild : T
        {
            var sourced = await LoadOrDefault<TChild>(identity, causationId);
            
            if (null == sourced) {
                throw new InvalidStateException(
                    $"Expected state {typeof(TChild)}, but got null"
                );
            }
            return sourced;
        }
    }
}
