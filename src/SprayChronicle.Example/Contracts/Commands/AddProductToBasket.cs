namespace SprayChronicle.Example.Contracts.Commands
{
    public sealed class AddProductToBasket
    {
        public readonly string BasketId;

        public readonly string ProductId;

        public AddProductToBasket(string basketId, string productId)
        {
            BasketId = basketId;
            ProductId = productId;
        }
    }
}
