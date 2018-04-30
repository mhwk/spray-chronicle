namespace SprayChronicle.EventSourcing
{
    public sealed class CatchUpOptions
    {
        public StreamOptions StreamOptions { get; }
        
        public long Checkpoint { get; }
        
        public string CausationId { get; }

        public CatchUpOptions(string streamName)
            : this(new StreamOptions(streamName), -1, null)
        {
            
        }

        public CatchUpOptions(StreamOptions streamOptions)
            : this(streamOptions, -1, null)
        {
            
        }

        private CatchUpOptions(StreamOptions streamOptions, long checkpoint, string causationId)
        {
            StreamOptions = streamOptions;
            Checkpoint = checkpoint;
            CausationId = causationId;
        }

        public CatchUpOptions WithCheckpoint(long checkpoint)
        {
            return new CatchUpOptions(StreamOptions, checkpoint, CausationId);
        }

        public CatchUpOptions WithCausationId(string causationId)
        {
            return new CatchUpOptions(StreamOptions, Checkpoint, causationId);
        }
    }
}
