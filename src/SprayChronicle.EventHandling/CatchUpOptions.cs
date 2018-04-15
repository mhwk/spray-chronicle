namespace SprayChronicle.EventHandling
{
    public sealed class CatchUpOptions
    {
        public string StreamName { get; }

        public CatchUpOptions(string streamName)
        {
            StreamName = streamName;
        }
    }
}
