using SprayChronicle.Server.Http;
using System.ComponentModel.DataAnnotations;

namespace SprayChronicle.Example.Contracts.Commands
{
    [HttpCommandAttribute("basket/check-out")]
    public sealed class CheckOutBasket
    {
        [Required()]
        public string BasketId { get; private set; }

        [Required()]
        public string OrderId { get; private set; }

        public CheckOutBasket(string basketId, string orderId)
        {
            BasketId = basketId;
            OrderId = orderId;
        }
    }
}
