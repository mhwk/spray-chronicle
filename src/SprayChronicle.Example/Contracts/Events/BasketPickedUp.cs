namespace SprayChronicle.Example.Contracts.Events
{
    public sealed class BasketPickedUp
    {
        public readonly string BasketId;

        public BasketPickedUp(string basketId)
        {
            BasketId = basketId;
        }
    }
}
