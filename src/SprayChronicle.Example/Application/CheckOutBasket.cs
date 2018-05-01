using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Application
{
    [HttpCommand("basket/{basketId}/check-out")]
    public sealed class CheckOutBasket
    {
        [Required]
        public string BasketId { get; }

        [Required]
        public string OrderId { get; }

        public CheckOutBasket(string basketId, string orderId)
        {
            BasketId = basketId;
            OrderId = orderId;
        }
    }
}
