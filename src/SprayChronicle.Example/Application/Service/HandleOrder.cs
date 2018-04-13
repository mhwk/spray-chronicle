using System;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using System.Linq;
using System.Threading.Tasks;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class HandleOrder : CommandHandler<HandleOrder,Order>
    {
        public HandleOrder(IEventSourcingRepository<Order> repository) : base(repository)
        {
        }

        private async Task Handle(GenerateOrder command)
        {
            await For(command.OrderId)
                .Mutate(() => Order.Generate(
                    command.OrderId,
                    command.ProductIds.Cast<ProductId>().ToArray()
                ));
        }
        
        private async Task Process(BasketCheckedOut message, DateTime epoch)
        {
            await Handle(new GenerateOrder(
                message.OrderId,
                message.ProductIds
            ));
        }
    }
}
