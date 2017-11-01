namespace SprayChronicle.Example.Domain
{
    public sealed class BasketCheckedOut
    {
        public readonly string BasketId;

        public readonly string OrderId;

        public readonly string[] ProductIds;

        public BasketCheckedOut(string basketId, string orderId, string[] productIds)
        {
            BasketId = basketId;
            OrderId = orderId;
            ProductIds = productIds;
        }
    }
}
