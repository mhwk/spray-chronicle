
namespace SprayChronicle.Example.Domain.Model
{
    public sealed class ProductId : Identifier
    {
        public ProductId(string id): base(id)
        {}
        
        public static implicit operator ProductId(string productId)
        {
            return new ProductId(productId);
        }
    }
}
