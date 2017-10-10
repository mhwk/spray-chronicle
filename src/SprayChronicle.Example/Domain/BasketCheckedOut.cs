namespace SprayChronicle.Example.Domain
{
    public sealed class BasketCheckedOut
    {
        public readonly string BasketId;

        public readonly string OrderId;

        public BasketCheckedOut(string basketId, string orderId)
        {
            BasketId = basketId;
            OrderId = orderId;
        }
    }
}
