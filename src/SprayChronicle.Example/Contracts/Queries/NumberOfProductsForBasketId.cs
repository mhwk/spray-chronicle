using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Contracts.Queries
{
    [HttpQueryAttribute("basket/number-of-products")]
    public class NumberOfProductsForBasketId
    {
        [Required]
        public string BasketId { get; private set; }

        public NumberOfProductsForBasketId(string basketId)
        {
            BasketId = basketId;
        }
    }
}
