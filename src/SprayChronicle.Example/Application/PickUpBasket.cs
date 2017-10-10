using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Application
{
    [HttpCommandAttribute("basket/{basketId}/pick-up")]
    public sealed class PickUpBasket
    {
        [Required]
        public string BasketId { get; }

        public PickUpBasket(string basketId)
        {
            BasketId = basketId;
        }
    }
}