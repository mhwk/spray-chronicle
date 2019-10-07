using Microsoft.Extensions.Options;

namespace SprayChronicle.Mongo
{
    public class MongoOptions
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string Database { get; set; } = "app";
        public string EventCollection { get; set; } = "EventsV2";
        public string EventCollectionOld { get; set; }
        public string SnapshotCollection { get; set; } = "Snapshots";
    }
}
