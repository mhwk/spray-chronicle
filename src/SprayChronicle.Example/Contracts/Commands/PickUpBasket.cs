namespace SprayChronicle.Example.Contracts.Commands
{
    public sealed class PickUpBasket
    {
        public readonly string BasketId;

        public PickUpBasket(string basketId)
        {
            BasketId = basketId;
        }
    }
}
