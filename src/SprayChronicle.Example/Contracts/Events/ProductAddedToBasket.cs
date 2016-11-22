namespace SprayChronicle.Example.Contracts.Events
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
