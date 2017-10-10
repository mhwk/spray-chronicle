namespace SprayChronicle.Example.Domain
{
    public sealed class ProductAddedToBasket
    {
        public readonly string BasketId;

        public readonly string ProductId;

        public ProductAddedToBasket(string basketId, string productId)
        {
            BasketId = basketId;
            ProductId = productId;
        }
    }
}
