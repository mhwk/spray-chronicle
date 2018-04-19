namespace SprayChronicle.EventSourcing
{
    public class ReadForwardOptions
    {
        public string StreamName { get; }
        
        public long Checkpoint { get; }

        public ReadForwardOptions(string streamName)
        {
            StreamName = streamName;
            Checkpoint = 0;
        }

        public ReadForwardOptions(string streamName, long checkpoint)
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
