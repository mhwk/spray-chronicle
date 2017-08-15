using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Contracts.Commands
{
    [HttpCommandAttribute("basket/remove-product")]
    public sealed class RemoveProductFromBasket
    {
        [Required]
        public string BasketId { get; }

        [Required]
        public string ProductId { get; }

        public RemoveProductFromBasket(string basketId, string productId)
        {
            BasketId = basketId;
            ProductId = productId;
        }
    }
}
