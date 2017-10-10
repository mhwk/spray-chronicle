using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Application
{
    [HttpQueryAttribute("basket/{basketId}/number-of-products")]
    public class NumberOfProductsForBasketId
    {
        [Required]
        public string BasketId { get; }

        public NumberOfProductsForBasketId(string basketId)
        {
            BasketId = basketId;
        }
    }
}
