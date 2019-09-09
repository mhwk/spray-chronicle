namespace SprayChronicle.Test.Example.Domain
{
    public class ProductChosen
    {
        public string CustomerId { get; private set; }
        public string ProductId { get; private set; }

        public ProductChosen(string customerId, string productId)
        {
            CustomerId = customerId;
            ProductId = productId;
        }
    }
}
