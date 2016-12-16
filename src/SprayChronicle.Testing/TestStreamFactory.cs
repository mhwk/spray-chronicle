using SprayChronicle.EventHandling;

namespace SprayChronicle.Testing
{
    public class TestStreamFactory : IBuildStreams
    {
        readonly TestStream _stream;

        public TestStreamFactory(TestStream stream)
        {
            _stream = stream;
        }

        public IStream CatchUp(string streamName, ILocateTypes typeLocator)
        {
            return _stream;
        }

        public IStream Persistent(string streamName, string groupName, ILocateTypes typeLocator)
        {
            return _stream;
        }
    }
}