using MongoDB.Bson.Serialization.Attributes;

namespace SprayChronicle.Mongo
{
    public sealed class MongoCheckpoint
    {
        [BsonId]
        public string Id { get; set; }
        public long Value { get; set; }
    }
}
