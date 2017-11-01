namespace SprayChronicle.Example.Domain
{
    public sealed class OrderGenerated
    {
        public readonly string OrderId;

        public readonly string[] ProductIds;

        public OrderGenerated(string orderId, string[] productIds)
        {
            OrderId = orderId;
            ProductIds = productIds;
        }
    }
}
