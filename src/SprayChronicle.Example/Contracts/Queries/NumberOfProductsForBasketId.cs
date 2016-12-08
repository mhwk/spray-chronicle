using SprayChronicle.Server.Http;

namespace SprayChronicle.Example.Contracts.Queries
{
    [HttpQueryAttribute("basket/number-of-products")]
    public class NumberOfProductsForBasketId
    {
        public readonly string BasketId;

        public NumberOfProductsForBasketId(string basketId)
        {
            BasketId = basketId;
        }
    }
}
