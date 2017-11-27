using SprayChronicle.EventHandling;

namespace SprayChronicle.Testing
{
    public class TestStreamFactory : IBuildStreams
    {
        private readonly TestStream _stream;

        public TestStreamFactory(TestStream stream)
        {
            _stream = stream;
        }

        public IStream CatchUp(string streamName)
        {
            return _stream;
        }

        public IStream Persistent(string streamName, string groupName)
        {
            return _stream;
        }
    }
}