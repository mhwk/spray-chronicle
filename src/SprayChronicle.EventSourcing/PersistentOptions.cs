namespace SprayChronicle.EventSourcing
{
    public sealed class PersistentOptions
    {
        public StreamOptions StreamOptions { get; }
        
        public string GroupName { get; }
        
        public string CausationId { get; }

        public PersistentOptions(string streamName, string groupName)
            : this(new StreamOptions(streamName), groupName, null)
        {
        }

        private PersistentOptions(StreamOptions streamOptions, string groupName, string causationId)
        {
            StreamOptions = streamOptions;
            GroupName = groupName;
            CausationId = causationId;
        }

        public PersistentOptions WithCausationId(string causationId)
        {
            return new PersistentOptions(
                StreamOptions,
                GroupName,
                causationId
            );
        }
    }
}
