using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Contracts.Commands
{
    [HttpCommandAttribute("basket/add-product")]
    public sealed class AddProductToBasket
    {
        [Required()]
        public string BasketId { get; private set; }

        [Required()]
        public string ProductId { get; private set; }

        public AddProductToBasket(string basketId, string productId)
        {
            BasketId = basketId;
            ProductId = productId;
        }
    }
}
