using SprayChronicle.Server.Http;

namespace SprayChronicle.Example.Contracts.Commands
{
    [HttpCommandAttribute("basket/add-product")]
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
