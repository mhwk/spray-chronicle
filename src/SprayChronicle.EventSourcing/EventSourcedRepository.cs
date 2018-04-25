using System;
using System.Threading.Tasks;

namespace SprayChronicle.EventSourcing
{
    public sealed class EventSourcedRepository<T> : IEventSourcingRepository<T> where T : EventSourced<T>
    {
        private readonly IEventStore _persistence;

        public EventSourcedRepository(IEventStore persistence)
        {
            _persistence = persistence;
        }

        public Task Save(T subject)
        {
            if (null == subject.Identity() || "" == subject.Identity()) {
                throw new UnknownStreamException("No identity found after apply of events");
            }
            _persistence.Append<T>(subject.Identity(), subject.Diff());

            return Task.CompletedTask;
        }

        public async Task Save<TChild>(T sourced) where TChild : T
        {
            if ( ! (sourced is TChild)) {
                throw new InvalidStateException(string.Format(
                    "Expected state {0}, but got {1}",
                    typeof(TChild),
                    sourced.GetType()
                ));
            }
            await Save(sourced);
        }

        public async Task<T> Load(string identity)
        {
            return await EventSourced<T>.Patch(_persistence.Load<T>(identity));
        }

        public async Task<TChild> LoadOrDefault<TChild>(string identity) where TChild : T
        {
            var sourced = await Load(identity);
            if (null == sourced) {
                return null;
            }
            if ( ! (sourced is TChild)) {
                throw new InvalidStateException(string.Format(
                    "Expected state {0}, but got {1}",
                    typeof(TChild),
                    sourced.GetType()
                ));
            }
            return (TChild) sourced;
        }

        public async Task<TChild> Load<TChild>(string identity) where TChild : T
        {
            var sourced = await LoadOrDefault<TChild>(identity);
            if (null == sourced) {
                throw new InvalidStateException(string.Format(
                    "Expected state {0}, but got null",
                    typeof(TChild)
                ));
            }
            return sourced;
        }
    }
}
