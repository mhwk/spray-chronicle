using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryStreamFactory : IBuildStreams
    {
        private readonly MemoryEventStore _eventStore;

        public MemoryStreamFactory(MemoryEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public IStream CatchUp(string reference)
        {
            return new MemoryStream(_eventStore);
        }

        public IStream Persistent(string reference, string category)
        {
            return new MemoryStream(_eventStore);
        }
    }
}