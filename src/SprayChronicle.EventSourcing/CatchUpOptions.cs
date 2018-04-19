namespace SprayChronicle.EventSourcing
{
    public sealed class CatchUpOptions
    {
        public string StreamName { get; }
        
        public long Checkpoint { get; }

        public CatchUpOptions(string streamName)
        {
            StreamName = streamName;
            Checkpoint = 0;
        }

        public CatchUpOptions(string streamName, long checkpoint)
        {
            StreamName = streamName;
            Checkpoint = checkpoint;
        }

        public CatchUpOptions WithCheckpoint(long checkpoint)
        {
            return new CatchUpOptions(StreamName, checkpoint);
        }
    }
}
