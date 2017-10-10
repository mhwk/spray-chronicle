namespace SprayChronicle.Example.Domain
{
    public sealed class ProductRemovedFromBasket
    {
        public readonly string BasketId;

        public readonly string ProductId;

        public ProductRemovedFromBasket(string basketId, string productId)
        {
            BasketId = basketId;
            ProductId = productId;
        }
    }
}
