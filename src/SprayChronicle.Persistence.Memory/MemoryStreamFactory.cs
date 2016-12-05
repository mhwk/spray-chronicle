using SprayChronicle.EventHandling;

namespace SprayChronicle.Persistence.Memory
{
    public class MemoryStreamFactory : IBuildStreams
    {
        MemoryEventStore _eventStore;

        public MemoryStreamFactory(MemoryEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public IStream CatchUp(string reference, string @namespace)
        {
            return new MemoryStream(_eventStore);
        }

        public IStream Persistent(string reference, string category, string @namespace)
        {
            return new MemoryStream(_eventStore);
        }
    }
}