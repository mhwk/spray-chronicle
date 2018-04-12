
namespace SprayChronicle.Example.Domain.Model
{
    public sealed class OrderId : Identifier
    {
        public OrderId(string id): base(id)
        {}
        
        public static implicit operator OrderId(string orderId)
        {
            return new OrderId(orderId);
        }
    }
}
