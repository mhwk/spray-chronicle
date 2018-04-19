using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Example.Domain.Model
{
    public sealed class Order : EventSourced<Order>
    {
        private readonly OrderId _orderId;

        private readonly ProductId[] _productIds;

        private Order(OrderId orderId, ProductId[] productIds)
        {
            _orderId = orderId;
            _productIds = productIds;
        }

        public static async Task<Order> Generate(OrderId orderId, ProductId[] productIds)
        {
            return await Apply(new OrderGenerated(
                orderId,
                productIds.Select(p => p.ToString()).ToArray()
            ));
        }

        private static Order On(OrderGenerated message)
        {
            return new Order(
                new OrderId(message.OrderId),
                message.ProductIds.Select(p => new ProductId(p)).ToArray()
            );
        }

        public override string Identity()
        {
            return _orderId;
        }
    }
}
