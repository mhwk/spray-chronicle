using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain.Model;
using System.Linq;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class OrderHandler : OverloadCommandHandler<Order>
    {
        public OrderHandler(IEventSourcingRepository<Order> repository) : base(repository)
        {
        }

        public void On(GenerateOrder command)
        {
            Start(() => Order.Generate(
                new OrderId(command.OrderId),
                command.ProductIds.Select(id => new ProductId(id)).ToArray()
            ));
        }
    }
}
