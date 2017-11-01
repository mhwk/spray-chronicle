namespace SprayChronicle.Example.Application
{
    public sealed class GenerateOrder
    {
        public readonly string OrderId;

        public readonly string[] ProductIds;

        public GenerateOrder(string orderId, string[] productIds)
        {
            OrderId = orderId;
            ProductIds = productIds;
        }
    }
}
