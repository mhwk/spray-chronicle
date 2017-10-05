namespace SprayChronicle.EventSourcing
{
    public class EventSourcedRepository<T> : IEventSourcingRepository<T> where T : EventSourced<T>
    {
        private readonly IEventStore _persistence;

        public EventSourcedRepository(IEventStore persistence)
        {
            _persistence = persistence;
        }

        public void Save(T subject)
        {
            if (null == subject.Identity() || "" == subject.Identity()) {
                throw new UnknownStreamException("No identity found after apply of events");
            }
            _persistence.Append<T>(subject.Identity(), subject.Diff());
        }

        public void Save<TChild>(T sourced) where TChild : T
        {
            if ( ! (sourced is TChild)) {
                throw new InvalidStateException(string.Format(
                    "Expected state {0}, but got {1}",
                    typeof(TChild),
                    sourced.GetType()
                ));
            }
            Save(sourced);
        }

        public T Load(string identity)
        {
            return EventSourced<T>.Patch(_persistence.Load<T>(identity));
        }

        public TChild Load<TChild>(string identity) where TChild : T
        {
            var sourced = Load(identity);
            if (null == sourced) {
                throw new InvalidStateException(string.Format(
                    "Expected state {0}, but got null",
                    typeof(TChild)
                ));
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
    }
}
