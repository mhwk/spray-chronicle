using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Application
{
    [HttpCommandAttribute("basket/{basketId}/add-product/{productId}")]
    public sealed class AddProductToBasket
    {
        [Required]
        public string BasketId { get; }

        [Required]
        public string ProductId { get; }

        public AddProductToBasket(string basketId, string productId)
        {
            BasketId = basketId;
            ProductId = productId;
        }
    }
}
