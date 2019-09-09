namespace SprayChronicle.Test.Example.Application
{
    public class ChooseProduct
    {
        public string CustomerId { get; private set; }
        public string ProductId { get; private set; }

        public ChooseProduct(string customerId, string productId)
        {
            CustomerId = customerId;
            ProductId = productId;
        }
    }
}
