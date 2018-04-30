namespace SprayChronicle.EventSourcing
{
    public sealed class CatchUpOptions
    {
        public string StreamName { get; }
        
        public long Checkpoint { get; }
        
        public string CausationId { get; }

        public CatchUpOptions(string streamName)
            : this(streamName, -1, null)
        {
            
        }

        private CatchUpOptions(string streamName, long checkpoint, string causationId)
        {
            StreamName = streamName;
            Checkpoint = checkpoint;
            CausationId = causationId;
        }

        public CatchUpOptions WithCheckpoint(long checkpoint)
        {
            return new CatchUpOptions(StreamName, checkpoint, CausationId);
        }

        public CatchUpOptions WithCausationId(string causationId)
        {
            return new CatchUpOptions(StreamName, Checkpoint, causationId);
        }
    }
}
