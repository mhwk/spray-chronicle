using SprayChronicle.Server.Http;

namespace SprayChronicle.Example.Contracts.Commands
{
    [HttpCommandAttribute("basket/pick-up")]
    public sealed class PickUpBasket
    {
        public readonly string BasketId;

        public PickUpBasket(string basketId)
        {
            BasketId = basketId;
        }
    }
}
