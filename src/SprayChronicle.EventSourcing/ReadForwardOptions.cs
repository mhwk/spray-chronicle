namespace SprayChronicle.EventSourcing
{
    public class ReadForwardOptions
    {
        public string StreamName { get; }

        public long Checkpoint { get; }
        
        public string CausationId { get; }

        public ReadForwardOptions(string streamName)
        {
            StreamName = streamName;
            Checkpoint = 0;
        }

        private ReadForwardOptions(string streamName, long checkpoint, string causationId)
        {
            StreamName = streamName;
            Checkpoint = checkpoint;
            CausationId = causationId;
        }

        public ReadForwardOptions WithCheckpoint(long checkpoint)
        {
            return new ReadForwardOptions(StreamName, checkpoint, CausationId);
        }

        public ReadForwardOptions WithCausationId(string causationId)
        {
            return new ReadForwardOptions(StreamName, Checkpoint, causationId);
        }
    }
}
