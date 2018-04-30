namespace SprayChronicle.EventSourcing
{
    public sealed class PersistentOptions
    {
        public string StreamName { get; }
        
        public string GroupName { get; }
        
        public string CausationId { get; }

        public PersistentOptions(string streamName, string groupName)
            : this(streamName, groupName, null)
        {
        }

        private PersistentOptions(string streamName, string groupName, string causationId)
        {
            StreamName = streamName;
            GroupName = groupName;
            CausationId = causationId;
        }

        public PersistentOptions WithCausationId(string causationId)
        {
            return new PersistentOptions(
                StreamName,
                GroupName,
                causationId
            );
        }
    }
}
