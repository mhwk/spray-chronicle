using System;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using System.Linq;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class HandleOrder : CommandHandler<Order>
    {
        public HandleOrder(IEventSourcingRepository<Order> repository) : base(repository)
        {
        }

        private void Handle(GenerateOrder command)
        {
            Repository().Start(() => Order.Generate(
                new OrderId(command.OrderId),
                command.ProductIds.Select(id => new ProductId(id)).ToArray()
            ));
        }
        
        private void Process(BasketCheckedOut message, DateTime epoch)
        {
            Handle(new GenerateOrder(
                message.OrderId,
                message.ProductIds
            ));
        }
    }
}
