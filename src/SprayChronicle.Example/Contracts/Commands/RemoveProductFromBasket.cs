namespace SprayChronicle.Example.Contracts.Commands
{
    public sealed class RemoveProductFromBasket
    {
        public readonly string BasketId;

        public readonly string ProductId;

        public RemoveProductFromBasket(string basketId, string productId)
        {
            BasketId = basketId;
            ProductId = productId;
        }
    }
}
