namespace SprayChronicle.EventSourcing
{
    public sealed class PersistentOptions
    {
        public string StreamName { get; }
        
        public string GroupName { get; }

        public PersistentOptions(string streamName, string groupName)
        {
            StreamName = streamName;
            GroupName = groupName;
        }
    }
}
