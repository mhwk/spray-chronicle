using SprayChronicle.Persistence.Memory;
using MongoDB.Bson.Serialization.Attributes;

namespace SprayChronicle.Example.Domain.State
{
    public class NumberOfProductsInBasket
    {
        [IdentifierAttribute, BsonIdAttribute]
        public readonly string BasketId;

        [BsonElementAttribute]
        public readonly int ProductCount;

        public NumberOfProductsInBasket(string basketId): this(basketId, 0)
        {}

        [BsonConstructorAttribute]
        public NumberOfProductsInBasket(string basketId, int productCount)
        {
            BasketId = basketId;
            ProductCount = productCount;
        }

        public NumberOfProductsInBasket Increase()
        {
            return new NumberOfProductsInBasket(BasketId, ProductCount + 1);
        }

        public NumberOfProductsInBasket Decrease()
        {
            return new NumberOfProductsInBasket(BasketId, ProductCount - 1);
        }
    }
}