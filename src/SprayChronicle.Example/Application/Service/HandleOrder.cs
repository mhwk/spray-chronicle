using System;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.EventHandling;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class HandleOrder : CommandHandler<HandleOrder,Order>,
        IHandle<GenerateOrder>,
        IProcess<BasketCheckedOut>
    {
        public async Task<CommandHandled> Handle(GenerateOrder command)
        {
            return await Handle(command.OrderId)
                .Mutate(() => Order.Generate(
                    command.OrderId,
                    command.ProductIds.Cast<ProductId>().ToArray()
                ));
        }
        
        public async Task<EventProcessed> Process(BasketCheckedOut message, DateTime epoch)
        {
            return await Dispatch(new GenerateOrder(
                message.OrderId,
                message.ProductIds
            ));
        }
    }
}
