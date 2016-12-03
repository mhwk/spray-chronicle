using SprayChronicle.Server.Http;

namespace SprayChronicle.Example.Contracts.Commands
{
    [HttpCommandAttribute("basket/remove-product")]
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
