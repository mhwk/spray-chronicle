using System.Linq;
using SprayChronicle.EventSourcing;

namespace SprayChronicle.Example.Domain.Model
{
    public sealed class Order : EventSourced<Order>
    {
        readonly OrderId _orderId;

        readonly ProductId[] _productIds;

        Order(OrderId orderId, ProductId[] productIds)
        {
            _orderId = orderId;
            _productIds = productIds;
        }

        public static Order Generate(OrderId orderId, ProductId[] productIds)
        {
            return Apply(new OrderGenerated(
                orderId,
                productIds.Select(p => p.ToString()).ToArray()
            ));
        }

        static Order On(OrderGenerated message)
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
