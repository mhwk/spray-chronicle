using MongoDB.Bson.Serialization.Attributes;
using SprayChronicle.Persistence.Memory;

namespace SprayChronicle.Example.Application.Model
{
    public sealed class PickedUpBasketsPerDay
    {
        [BsonIdAttribute]
        [IdentifierAttribute]
        public readonly string Day;

        [BsonElementAttribute]
        public readonly int Count;

        [BsonConstructorAttribute]
        public PickedUpBasketsPerDay(string day, int count)
        {
            Day = day;
            Count = count;
        }

        public PickedUpBasketsPerDay(string day): this(day, 0)
        {}

        public PickedUpBasketsPerDay Increase()
        {
            return new PickedUpBasketsPerDay(
                Day,
                Count + 1
            );
        }
    }
}