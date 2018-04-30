namespace SprayChronicle.EventSourcing
{
    public class ReadForwardOptions
    {
        public StreamOptions StreamOptions { get; }

        public long Checkpoint { get; }
        
        public string CausationId { get; }

        public ReadForwardOptions(string streamName)
            : this(new StreamOptions(streamName), -1, null)
        {
        }

        private ReadForwardOptions(StreamOptions streamOptions, long checkpoint, string causationId)
        {
            StreamOptions = streamOptions;
            Checkpoint = checkpoint;
            CausationId = causationId;
        }

        public ReadForwardOptions WithCheckpoint(long checkpoint)
        {
            return new ReadForwardOptions(StreamOptions, checkpoint, CausationId);
        }

        public ReadForwardOptions WithCausationId(string causationId)
        {
            return new ReadForwardOptions(StreamOptions, Checkpoint, causationId);
        }
    }
}
